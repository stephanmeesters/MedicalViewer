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
    public class Window : GameWindow
    {
        public enum RenderMode : int
        {
            Regular = 1,
            ObjectID = 2,
            Outline = 3
        }

        private Shader _shader;
        private Shader _shaderPlane;
        private Shader _shaderWidget;

        private Texture3D _tex3D;

        private List<Model> _models = new List<Model>();
        private List<Model> _planes = new List<Model>();
        private List<Model> _widgets = new List<Model>();

        private Camera _camera;

        Stopwatch _sw;

        private bool _firstMove = true;
        private Vector2 _lastPos;
        private int cval;
        private long _prevElapsedTime;
        private float[] _previousFPS = new float[100];
        private int _previousFPSIndex;

        ///

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

        string[] _modelTitles =
        {
            "myocardium of the left ventricle",
            "left atrium blood cavity",
            "left ventricle blood cavity",
            "right atrium blood cavity",
            "right ventricle blood cavity",
            "pulmonary artery",
            "ascending aorta",
            "rotation x",
            "rotation y",
            "rotation z"
        };

        ///

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            // window settings
            CursorGrabbed = true;

            // start with black screen
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            SwapBuffers();

            // load textures
            _tex3D = Texture3D.LoadFromFile("Data/CT/ct_image.nii.gz");
            //_tex3D = Texture3D.LoadFromFile("Data/MR/mr_image_reg.nii.gz");
            
            // compile shaders
            _shader = new Shader("Shaders/vshader.glsl", "Shaders/fshader.glsl");
            _shaderPlane = new Shader("Shaders/vshader.glsl", "Shaders/fshader_plane.glsl");
            _shaderWidget = new Shader("Shaders/vshader_widget.glsl", "Shaders/fshader_widget.glsl");


            // set camera
            Vector3 camPos = new Vector3
            {
                X = 0.0f,
                Y = 0.0f,
                Z = 3.0f
            };
            _camera = new Camera(camPos, Size.X / (float)Size.Y);

            // load anatomical models
            // load sform or qform
            Matrix4 modelTransform = _tex3D.Transformation;
            modelTransform.Transpose();
            // transform to world coordinates
            modelTransform.Invert();
            // normalize coordinates within 0-1 range
            modelTransform *= Matrix4.CreateScale(1.0f / 512.0f, 1.0f / 512.0f, 1.0f / 363.0f);

            Random rand = new Random();
            float objectID = 0.0f;
            foreach (string f in modelFiles)
            {
                objectID += 10.0f;

                Model m = Model.Load(f);
                m.ConstructVAO();
                m.BindToShader(_shader);
                m.transform = modelTransform;
                m.color = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                m.objectID = objectID;
                m.CalculateCenterOfMass();
                _models.Add(m);
            }

            // create anatomical planes
            var plane_x = Model.Load("Data/plane.obj");
            plane_x.ConstructVAO();
            plane_x.BindToShader(_shaderPlane);
            plane_x.transform = Matrix4.CreateTranslation(0.0f, 0.0f, 0.5f);
            plane_x.name = "Plane_X";
            plane_x.visible = false;
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

            // create widgets
            float widgetScale = 0.75f;

            objectID += 10.0f;
            var rotation_widget_x = Model.Load("Data/rotation_widget.obj");
            rotation_widget_x.ConstructVAO();
            rotation_widget_x.BindToShader(_shaderWidget);
            rotation_widget_x.color = new Vector3(1.0f, 0.0f, 0.0f);
            rotation_widget_x.CalculateCenterOfMass();
            rotation_widget_x.transform = Matrix4.CreateScale(widgetScale);
            rotation_widget_x.transform *= Matrix4.CreateRotationY((float)Math.PI / 2.0f);
            rotation_widget_x.transform *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            rotation_widget_x.objectID = objectID;
            _widgets.Add(rotation_widget_x);

            objectID += 10.0f;
            var rotation_widget_y = Model.Load("Data/rotation_widget.obj");
            rotation_widget_y.ConstructVAO();
            rotation_widget_y.BindToShader(_shaderWidget);
            rotation_widget_y.color = new Vector3(0.0f, 1.0f, 0.0f);
            rotation_widget_y.CalculateCenterOfMass();
            rotation_widget_y.transform = Matrix4.CreateScale(widgetScale);
            rotation_widget_y.transform *= Matrix4.CreateRotationX((float)Math.PI / 2.0f);
            rotation_widget_y.transform *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            rotation_widget_y.objectID = objectID;
            _widgets.Add(rotation_widget_y);

            objectID += 10.0f;
            var rotation_widget_z = Model.Load("Data/rotation_widget.obj");
            rotation_widget_z.ConstructVAO();
            rotation_widget_z.BindToShader(_shaderWidget);
            //rotation_widget_x.transform = Matrix4.Identity;
            rotation_widget_z.color = new Vector3(0.0f, 0.0f, 1.0f);
            rotation_widget_z.CalculateCenterOfMass();
            rotation_widget_z.transform = Matrix4.CreateScale(widgetScale);
            rotation_widget_z.transform *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            rotation_widget_z.objectID = objectID;
            _widgets.Add(rotation_widget_z);

            // stopwatch for animations
            _sw = Stopwatch.StartNew();
            _sw.Start();
        }

        // Now that initialization is done, let's create our render loop.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // GL settings
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less);

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.StencilTest);

            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.StencilMask(0x00);

            //
            //  calculate transforms
            //

            float time = (float)(_sw.ElapsedMilliseconds) / 1000.0f;
            foreach (Model m in _models)
            {
                Vector4 centroidTrans = new Vector4(m.centerOfMass);
                Matrix4 mm = m.transform;
                mm *= Matrix4.CreateTranslation(-centroidTrans.X, -centroidTrans.Y, -centroidTrans.Z);
                mm *= Matrix4.CreateScale((float)(1.0 - 0.05 * (0.5 + 0.5 * Math.Sin(time * 0.8))));
                mm *= Matrix4.CreateTranslation(centroidTrans.X, centroidTrans.Y, centroidTrans.Z);

                Matrix4 mm2 = Matrix4.Identity;
                mm2 *= Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f);
                mm2 *= Matrix4.CreateRotationX(-90.0f);
                mm2 *= Matrix4.CreateRotationY(time);
                //mm2 *= Matrix4.CreateRotationZ(time);
                mm2 *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);

                m.frame_transform = mm;
                m.frame_transform2 = mm2;
            }

            //
            //  object id map
            //

            _shader.Use();
            
            _shader.SetInt("renderMode", (int)RenderMode.ObjectID);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            foreach (Model m in _models)
            {
                _shader.SetMatrix4("model", m.frame_transform);
                _shader.SetMatrix4("model2", m.frame_transform2);
                _shader.SetVector3("light.color", m.color);
                _shader.SetFloat("objectID", m.objectID / 255.0f);
                m.Draw();
            }

            _shaderWidget.Use();
            _shaderWidget.SetInt("renderMode", (int)RenderMode.ObjectID);
            foreach (Model m in _widgets)
            {
                _shaderWidget.SetMatrix4("model", m.transform);
                _shaderWidget.SetMatrix4("view", _camera.GetViewMatrix());
                _shaderWidget.SetMatrix4("projection", _camera.GetProjectionMatrix());
                _shaderWidget.SetFloat("objectID", m.objectID / 255.0f);
                m.Draw();
            }

            IntPtr Pixel = new IntPtr();
            GL.ReadPixels(600, 600, 1, 1, PixelFormat.Red, PixelType.UnsignedByte, ref Pixel);
            cval = (int)Pixel;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            //
            //  anatomical models
            //

            _tex3D.Use(TextureUnit.Texture0);

            _shader.SetVector3("viewPos", _camera.Position);
            _shader.SetVector3("light.direction", new Vector3(0.0f, 1.0f, 0.0f));
            _shader.SetVector3("light.ambient", new Vector3(0.2f));
            _shader.SetVector3("light.diffuse", new Vector3(0.8f));
            _shader.SetVector3("light.specular", new Vector3(1.0f));
            _shader.SetFloat("light.colorStrength", 0.0f);

            double norm = 65;// 1.0 / (_tex3D.ImageHighestIntensity - _tex3D.ImageLowestIntensity);
                             //_shader.SetFloat("minIntensity", (float)_tex3D.ImageLowestIntensity);
                             // _shader.SetFloat("maxIntensity", (float)_tex3D.ImageHighestIntensity);
            _shader.SetFloat("norm", (float)norm);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            _shader.SetInt("renderMode", (int)RenderMode.Regular);
            foreach (Model m in _models)
            {
                _shader.SetMatrix4("model", m.frame_transform);
                _shader.SetMatrix4("model2", m.frame_transform2);
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

            GL.Enable(EnableCap.CullFace);

            _shaderWidget.Use();
            foreach (Model m in _widgets)
            {
                _shaderWidget.SetInt("renderMode", (int)RenderMode.Regular);
                _shaderWidget.SetMatrix4("model", m.transform);
                _shaderWidget.SetMatrix4("view", _camera.GetViewMatrix());
                _shaderWidget.SetMatrix4("projection", _camera.GetProjectionMatrix());
                if(m.isSelected)
                    _shaderWidget.SetVector3("color", new Vector3(1.0f, 1.0f, 0.0f));
                else
                    _shaderWidget.SetVector3("color", m.color);
                m.Draw();
            }


            //
            //  selected object outline
            //

            float index = cval;
            foreach (Model m in _models)
            {
                if (m.objectID != index)
                    continue;

                _shader.Use();

                // disable writing to color space
                GL.ColorMask(false, false, false, false);
                GL.DepthMask(false);

                GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilMask(0xFF); // enable writing to stencil
                GL.Disable(EnableCap.DepthTest);

                _shader.SetInt("renderMode", (int)RenderMode.Regular);
                _shader.SetMatrix4("model", m.frame_transform);
                _shader.SetMatrix4("model2", m.frame_transform2);
                _shader.SetVector3("light.color", m.color);
                m.Draw();

                GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF); // only allow fragments outside the stencil
                GL.StencilMask(0x00); // disable writing to stencil

                // enable writing to color space
                GL.ColorMask(true, true, true, true);
                GL.DepthMask(true);

                _shader.SetInt("renderMode", (int)RenderMode.Outline);
                _shader.SetVector3("outlineColor", new Vector3(1.0f, 0.0f, 0.0f));
                m.Draw();

                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilMask(0x00);
                GL.Enable(EnableCap.DepthTest);
            }

            foreach (Model m in _widgets)
            {
                m.isSelected = (int)m.objectID == index;
            }

            SwapBuffers();

            GL.BindVertexArray(0);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            int index = cval / 10 - 1;
            index = Math.Clamp(index, 0, _modelTitles.Length - 1);

            long newTime = _sw.ElapsedMilliseconds;
            long diff = newTime - _prevElapsedTime;
            if (_prevElapsedTime != 0)
                this.Title = string.Format("Medical Viewer - frametime: {0} ms ({1} FPS) - {2}", diff, Math.Round(_previousFPS.Average()), _modelTitles[index]);
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
            foreach(Model m in _models)
            {
                m.Delete();
            }
            foreach (Model m in _planes)
            {
                m.Delete();
            }

            GL.DeleteProgram(_shader.Handle);
            GL.DeleteProgram(_shaderPlane.Handle);

            base.OnUnload();
        }
    }
}
