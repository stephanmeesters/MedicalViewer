using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using System;
using System.Linq;

namespace LearnOpenTK
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Window : GameWindow
    {
        // Create the vertices for our triangle. These are listed in normalized device coordinates (NDC)
        // In NDC, (0, 0) is the center of the screen.
        // Negative X coordinates move to the left, positive X move to the right.
        // Negative Y coordinates move to the bottom, positive Y move to the top.
        // OpenGL only supports rendering in 3D, so to create a flat triangle, the Z coordinate will be kept as 0.
        private readonly float[] _vertices =
        {
            // Positions          
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,

            0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
             -0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,

            -0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f,  0.5f,

             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,

             0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f,
        };

        /*private uint[] _indices =
        {
             0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };*/

        // These are the handles to OpenGL objects. A handle is an integer representing where the object lives on the
        // graphics card. Consider them sort of like a pointer; we can't do anything with them directly, but we can
        // send them to OpenGL functions that need them.

        // What these objects are will be explained in OnLoad.
        private int _vertexBufferObject;
        private int _vertexBufferObject_index;
        private int _vertexArrayObject;

        private int _vertexBufferObject_obj_vertex;
        private int _vertexBufferObject_obj_index;
        private int _vertexArrayObject_obj;
        private Mesh _mesh;

        // This class is a wrapper around a shader, which helps us manage it.
        // The shader class's code is in the Common project.
        // What shaders are and what they're used for will be explained later in this tutorial.
        private Shader _shader;
        private Shader _shaderPlane;

        private Texture3D _tex3D;

        private Camera _camera;

        Stopwatch _sw;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private long _prevElapsedTime;
        private float[] _previousFPS = new float[100];
        private int _previousFPSIndex;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }


        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();
            
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            SwapBuffers();

            _tex3D = Texture3D.LoadFromFile("Data/ct_anat_short.nii.gz");
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shader = new Shader("Shaders/vshader.glsl", "Shaders/fshader.glsl");
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shaderPlane = new Shader("Shaders/vshader_plane.glsl", "Shaders/fshader_plane.glsl");
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            /*_vertexBufferObject_index = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vertexBufferObject_index);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);*/

            var positionLocation = _shaderPlane.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);



            Vector3 camPos = new Vector3
            {
                X = 0.5f,
                Y = 0.5f,
                Z = 3.0f
            };
            _camera = new Camera(camPos, Size.X / (float)Size.Y);
            CursorGrabbed = true;

            





            _mesh = ObjLoader.Load("Data/model2.obj");

            _vertexArrayObject_obj = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject_obj);

            _vertexBufferObject_obj_vertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject_obj_vertex);
            GL.BufferData(BufferTarget.ArrayBuffer, _mesh.numberOfAttributes * sizeof(float), _mesh.vertices_attributes, BufferUsageHint.StaticDraw);

            _vertexBufferObject_obj_index = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vertexBufferObject_obj_index);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _mesh.numberOfIndices * sizeof(uint), _mesh.vertexIndices.ToArray(), BufferUsageHint.StaticDraw);

            positionLocation = _shader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, _mesh.stride * sizeof(float), 0);

            var normalLocation = _shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, true, _mesh.stride * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            _sw = Stopwatch.StartNew();
            _sw.Start();
        }

        // Now that initialization is done, let's create our render loop.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Front);

            //GL.Enable(EnableCap.Multisample);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //
            //  model 1
            //

            _tex3D.Use(TextureUnit.Texture0);
            _shader.Use();
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            float time = (float)(_sw.ElapsedMilliseconds)/1000.0f;

            Matrix4 qform = Matrix4.Identity;
            //qform.M11 = -0.355469f;

            qform.Row0 = new Vector4(-0.355469f, 0.0f, 0.0f, 45.5f);
            qform.Row1 = new Vector4(0.0f, 0.355469f, 0.0f, -228.585f);
            qform.Row2 = new Vector4(0.0f, 0.0f, 0.45f, -271.88f);

            qform.Invert();

            //Matrix4.CreateScale(10, 2.0, )

            //qform.Transpose();

            //Matrix4 model = Matrix4.CreateTranslation(45.5f, -228.585f, -271.88f);
            //model *= Matrix4.CreateScale(-0.355469f, 0.355469f, 0.45f);
            //model *= Matrix4.CreateScale(2.0f / 512.0f);

            Matrix4 model = Matrix4.Identity;
            
            model *= Matrix4.CreateTranslation(-45.5f, 228.585f, 271.88f);
            model *= Matrix4.CreateScale(1.0f/0.355469f, 1.0f/0.355469f, 1.0f/0.45f);
            model *= Matrix4.CreateScale(1.0f/512.0f, 1.0f/512.0f, 1.0f/363.0f);
            model *= Matrix4.CreateTranslation(1.0f, 0.0f, 0.0f);


            //Matrix4 model = qform;//Matrix4.CreateTranslation(-_mesh.centerOfMass.X, -_mesh.centerOfMass.Y, -_mesh.centerOfMass.Z);
            //model *= Matrix4.CreateRotationY(time * 0.25f);
            //model *= Matrix4.CreateScale( 2.0f / 512.0f);
            //model *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            //model *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            ///model *= Matrix4.CreateTranslation((float)Math.Sin(time)*0.25f, 0.0f, 0.0f);

            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shader.SetVector3("viewPos", _camera.Position);
            _shader.SetVector3("light.direction", new Vector3(0.0f, 1.0f, 0.0f));
            _shader.SetVector3("light.ambient", new Vector3(0.2f));
            _shader.SetVector3("light.diffuse", new Vector3(0.5f));
            _shader.SetVector3("light.specular", new Vector3(1.0f));
            _shader.SetFloat("light.base", 0.05f);

            double norm = 45;// 1.0 / (_tex3D.ImageHighestIntensity - _tex3D.ImageLowestIntensity);
            //_shader.SetFloat("minIntensity", (float)_tex3D.ImageLowestIntensity);
           // _shader.SetFloat("maxIntensity", (float)_tex3D.ImageHighestIntensity);
            _shader.SetFloat("norm", (float)norm);

            GL.BindVertexArray(_vertexArrayObject_obj);
            GL.DrawElements(PrimitiveType.Triangles, _mesh.numberOfIndices, DrawElementsType.UnsignedInt, 0);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            //
            //  model 2
            //

            _tex3D.Use(TextureUnit.Texture0);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shaderPlane.Use();
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            var model2 = Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            model2 *= Matrix4.CreateScale(1.0f);
            _shaderPlane.SetMatrix4("model", model2);
            _shaderPlane.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderPlane.SetMatrix4("projection", _camera.GetProjectionMatrix());
            _shaderPlane.SetFloat("norm", 45.0f);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            GL.BindVertexArray(_vertexArrayObject);
            //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawArrays(PrimitiveType.Lines, 0, 36);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            SwapBuffers();

            GL.BindVertexArray(0);

            //https://github.com/dabbertorres/ObjRenderer/blob/master/ObjRenderer/RenderTab.cs
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            
            long newTime = _sw.ElapsedMilliseconds;
            long diff = newTime - _prevElapsedTime;
            if (_prevElapsedTime != 0)
                this.Title = string.Format("Medical Viewer - frametime: {0} ms ({1} FPS)", diff, Math.Round(_previousFPS.Average()));
            _prevElapsedTime = newTime;
            if(diff > 0)
            {
                _previousFPS[_previousFPSIndex] = 1000.0f / diff;
                _previousFPSIndex++;
                if (_previousFPSIndex >= 100)
                    _previousFPSIndex = 0;
            }
            



            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_vertexBufferObject_index);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);

            base.OnUnload();
        }
    }
}
