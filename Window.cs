﻿using OpenTK.Graphics.OpenGL4;
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
        

        private List<Model> _models = new List<Model>();
        private List<Model> _planes = new List<Model>();

        private Shader _shader;
        private Shader _shaderPlane;

        private Texture3D _tex3D;

        private Camera _camera;

        Stopwatch _sw;


        private bool _firstMove = true;

        private Vector2 _lastPos;

        private int cval;

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

            // set camera
            Vector3 camPos = new Vector3
            {
                X = 0.5f,
                Y = 0.5f,
                Z = 3.0f
            };
            _camera = new Camera(camPos, Size.X / (float)Size.Y);

            // load anatomical models
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
                _models.Add(m);
            }

            // create anatomical planes
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
            GL.DepthFunc(DepthFunction.Lequal);

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            GL.Enable(EnableCap.Multisample);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //
            //  object id map
            //

            _shader.Use();
            
            float time = (float)(_sw.ElapsedMilliseconds) / 1000.0f;
            _shader.SetInt("normalRender", 0);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
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
                _shader.SetFloat("objectID", m.objectID / 255.0f);
                m.Draw();
            }

            IntPtr Pixel = new IntPtr();
            GL.ReadPixels(600, 600, 1, 1, PixelFormat.Red, PixelType.UnsignedByte, ref Pixel);
            cval = (int)Pixel;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
            _shader.SetInt("normalRender", 1);

            double norm = 65;// 1.0 / (_tex3D.ImageHighestIntensity - _tex3D.ImageLowestIntensity);
                             //_shader.SetFloat("minIntensity", (float)_tex3D.ImageLowestIntensity);
                             // _shader.SetFloat("maxIntensity", (float)_tex3D.ImageHighestIntensity);
            _shader.SetFloat("norm", (float)norm);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);
            _shader.SetInt("normalRender", 1);

            foreach (Model m in _models)
            {

                Vector4 centroidTrans = new Vector4(m.mesh.centerOfMass);
                centroidTrans *= m.transform;
                Matrix4 mm = m.transform;
                mm *= Matrix4.CreateTranslation(centroidTrans.X-1.0f, centroidTrans.Y, centroidTrans.Z);
                mm *= Matrix4.CreateScale((float)(1.0-0.02*(0.5 + 0.5*Math.Sin(time*0.8))));
                mm *= Matrix4.CreateTranslation(-centroidTrans.X+1.0f, -centroidTrans.Y, -centroidTrans.Z);

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
                this.Title = string.Format("Medical Viewer - frametime: {0} ms ({1} FPS) - {2}", diff, Math.Round(_previousFPS.Average()), cval);
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
