using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

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
            // first triangle
             -0.5f+0.5f,  0.5f+0.5f, 0.0f, 1.0f, 1.0f, 0.2f, 1.0f, // top right
             -0.5f+-0.5f, 0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.2f, 1.0f, // bottom left
            -0.5f+-0.5f,  0.5f+0.5f, 0.0f, 0.0f, 1.0f, 0.2f, 1.0f, // top left
            // second triangle
            -0.5f+0.5f, 0.5f+0.5f, 0.0f, 1.0f, 1.0f, 0.2f, 1.0f,  // top right
             -0.5f+-0.5f, 0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.2f, 1.0f,  // bottom left
            -0.5f+0.5f,  0.5f+-0.5f, 0.0f, 1.0f, 0.0f, 0.2f, 1.0f,   // bottom right

            // first triangle
             0.5f+0.5f,  0.5f+0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 2.0f, // top right
             0.5f+-0.5f, 0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 2.0f, // bottom left
            0.5f+-0.5f,  0.5f+0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 2.0f, // top left
            // second triangle
            0.5f+0.5f, 0.5f+0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 2.0f,  // top right
             0.5f+-0.5f, 0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 2.0f,  // bottom left
            0.5f+0.5f,  0.5f+-0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 2.0f,   // bottom right

            // first triangle
             -0.5f+0.5f,  -0.5f+0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 3.0f, // top right
             -0.5f+-0.5f, -0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 3.0f, // bottom left
            -0.5f+-0.5f,  -0.5f+0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 3.0f, // top left
            // second triangle
            -0.5f+0.5f, -0.5f+0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 3.0f,  // top right
             -0.5f+-0.5f, -0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 3.0f,  // bottom left
            -0.5f+0.5f,  -0.5f+-0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 3.0f,   // bottom right

            // first triangle
             0.5f+0.5f,  -0.5f+0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 4.0f, // top right
             0.5f+-0.5f, -0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 4.0f, // bottom left
            0.5f+-0.5f,  -0.5f+0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 4.0f, // top left
            // second triangle
            0.5f+0.5f, -0.5f+0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 4.0f,  // top right
             0.5f+-0.5f, -0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 4.0f,  // bottom left
            0.5f+0.5f,  -0.5f+-0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 4.0f   // bottom right
        };

        // These are the handles to OpenGL objects. A handle is an integer representing where the object lives on the
        // graphics card. Consider them sort of like a pointer; we can't do anything with them directly, but we can
        // send them to OpenGL functions that need them.

        // What these objects are will be explained in OnLoad.
        private int _vertexBufferObject;

        private int _vertexArrayObject;

        // This class is a wrapper around a shader, which helps us manage it.
        // The shader class's code is in the Common project.
        // What shaders are and what they're used for will be explained later in this tutorial.
        private Shader _shader;

        private Texture3D _tex3D;

        private Camera _camera;

        private float curSlice = 0;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();
            
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            _tex3D = Texture3D.LoadFromFile("Data/test.nii");
            _shader = new Shader("Shaders/vshader.glsl", "Shaders/fshader.glsl");

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            var positionLocation = _shader.GetAttribLocation("vertex");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("texcoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
        }

        // Now that initialization is done, let's create our render loop.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);
            _tex3D.Use(TextureUnit.Texture0);
            _shader.Use();

            //_shader.SetMatrix4("view", _camera.GetViewMatrix());
            //_shader.SetMatrix4("projMatrix", _camera.GetProjectionMatrix());
            //_shader.SetVector3("viewPos", _camera.Position);
            
            curSlice += (float)0.001;
            if (curSlice > 1.0)
                curSlice = 0;
            _shader.SetFloat("slice", curSlice);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 18);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
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
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);

            base.OnUnload();
        }
    }
}
