using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;

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

        private Model _model;
        private Model _model2;

        private List<Model> _models = new List<Model>();
        private List<Model> _planes = new List<Model>();
        private Model _cube;

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

            _tex3D = Texture3D.LoadFromFile("Data/CT/ct_image.nii.gz");
            //_tex3D = Texture3D.LoadFromFile("Data/MR/mr_image_reg.nii.gz");
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shader = new Shader("Shaders/vshader.glsl", "Shaders/fshader.glsl");
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shaderPlane = new Shader("Shaders/vshader.glsl", "Shaders/fshader_plane.glsl");
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
            //_camera.Pitch = 90;
            CursorGrabbed = true;

            string[] modelFiles =
            {
                "Data/CT/205.obj",
                "Data/CT/420.obj",
                "Data/CT/500.obj",
                "Data/CT/550.obj",
                "Data/CT/600.obj",
                "Data/CT/820.obj",
                "Data/CT/850.obj"
            };

            Matrix4 modelTransform = Matrix4.Identity;
            modelTransform *= Matrix4.CreateTranslation(-45.5f, 228.585f, 271.88f);
            modelTransform *= Matrix4.CreateScale(1.0f / 0.355469f, 1.0f / 0.355469f, 1.0f / 0.45f);
            modelTransform *= Matrix4.CreateScale(1.0f / 512.0f, 1.0f / 512.0f, 1.0f / 363.0f);
            modelTransform *= Matrix4.CreateTranslation(1.0f, 0.0f, 0.0f);

            Random rand = new Random();
            foreach (string f in modelFiles)
            {
                Model m = Model.Load(f);
                m.ConstructVAO();
                m.BindToShader(_shader);
                m.transform = modelTransform;
                m.color = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                _models.Add(m);
            }


            var plane_x = Model.Load("Data/plane.obj");
            plane_x.ConstructVAO();
            plane_x.BindToShader(_shaderPlane);
            plane_x.transform = Matrix4.CreateTranslation(0.0f, 0.0f, 0.5f);
            plane_x.name = "Plane_X";
            _planes.Add(plane_x);

            var plane_y = Model.Load("Data/plane.obj");
            plane_y.ConstructVAO();
            plane_y.BindToShader(_shaderPlane);
            plane_y.transform = Matrix4.CreateRotationY(-(float)Math.PI*0.5f);
            plane_y.visible = false;
            //plane_y.transform *= Matrix4.CreateTranslation(0.0f, 0.0f, 0.5f);
            _planes.Add(plane_y);

            var plane_z = Model.Load("Data/plane.obj");
            plane_z.ConstructVAO();
            plane_z.BindToShader(_shaderPlane);
            plane_z.transform = Matrix4.CreateRotationX((float)Math.PI * 0.5f);
            plane_z.visible = false;
            _planes.Add(plane_z);

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

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            GL.Enable(EnableCap.Multisample);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //
            //  anatomical models
            //

            
            _tex3D.Use(TextureUnit.Texture0);
            _shader.Use();
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            float time = (float)(_sw.ElapsedMilliseconds)/1000.0f;

            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shader.SetVector3("viewPos", _camera.Position);
            _shader.SetVector3("light.direction", new Vector3(0.0f, 1.0f, 0.0f));
            _shader.SetVector3("light.ambient", new Vector3(0.2f));
            _shader.SetVector3("light.diffuse", new Vector3(0.5f));
            _shader.SetVector3("light.specular", new Vector3(1.0f));
            _shader.SetFloat("light.colorStrength", 0.0f);

            double norm = 45;// 1.0 / (_tex3D.ImageHighestIntensity - _tex3D.ImageLowestIntensity);
            //_shader.SetFloat("minIntensity", (float)_tex3D.ImageLowestIntensity);
           // _shader.SetFloat("maxIntensity", (float)_tex3D.ImageHighestIntensity);
            _shader.SetFloat("norm", (float)norm);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            foreach (Model m in _models)
            {

                Vector4 centroidTrans = new Vector4(m.mesh.centerOfMass);
                centroidTrans *= m.transform;
                Matrix4 mm = m.transform;
                //mm *= Matrix4.CreateTranslation(centroidTrans.X, centroidTrans.Y, centroidTrans.Z);
                //mm *= Matrix4.CreateScale((float)(1.0-0.05*(0.5 + 0.5*Math.Sin(time*0.5))));
                //mm *= Matrix4.CreateTranslation(-centroidTrans.X, -centroidTrans.Y, -centroidTrans.Z);

                _shader.SetMatrix4("model", mm);
                _shader.SetVector3("light.color", m.color);
                m.Draw();
            }

            //
            //  cube model
            //

            GL.Disable(EnableCap.CullFace);

            _shaderPlane.Use();
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);
            
            _shaderPlane.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderPlane.SetMatrix4("projection", _camera.GetProjectionMatrix());
            norm = 45;
            _shaderPlane.SetFloat("norm", (float)norm);
            _shaderPlane.SetVector3("viewPos", _camera.Position);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            foreach (Model m in _planes)
            {
                if(m.name == "Plane_X")
                {
                    m.transform = Matrix4.CreateTranslation(0.0f, 0.0f, (float)(0.5 + 0.5 * Math.Sin(time * 0.5)));
                }
                _shaderPlane.SetMatrix4("model", m.transform);
                m.Draw();
            }

            //
            //  coordinate system
            //

            

            SwapBuffers();

            GL.BindVertexArray(0);

            //https://github.com/dabbertorres/ObjRenderer/blob/master/ObjRenderer/RenderTab.cs
            // https://dreamstatecoding.blogspot.com/2018/03/opengl-4-with-opentk-in-c-part-15.html
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
