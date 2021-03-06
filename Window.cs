using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MedViewer
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
        private Shader _shaderText;

        private Texture3D _tex3D;
        private Texture _texLoading;
        private Texture _texInstructions;

        private List<Model> _models = new List<Model>();
        private List<Model> _planes = new List<Model>();
        private List<Model> _widgets = new List<Model>();

        private TextImage _instructionsText;

        private Camera _camera;

        Stopwatch _sw;

        private float _xRot = -(float)Math.PI / 2.0f;
        private float _yRot = 0.0f;
        private float _zRot = 0.0f;
        private float _xPlaneTrans = 0.2f;
        private float _yPlaneTrans = 0.0f;
        private float _zPlaneTrans = 0.0f;

        private bool _bBreathing = true;
        private bool _bSpinning = true;

        private bool _firstMove = true;
        private Vector2 _lastPos;
        private int cval;
        private long _prevElapsedTime;
        private long _lastMouseMovedTime;
        private float[] _previousFPS = new float[100];
        private int _previousFPSIndex;
        private bool _hasActedOnClick = false;

        private string[] modelFiles =
        {
            "Data/CT/205.obj",
            "Data/CT/420.obj",
            "Data/CT/500.obj",
            "Data/CT/550.obj",
            "Data/CT/600.obj",
            "Data/CT/820.obj",
            "Data/CT/850.obj"
        };

        private string[] _modelTitles =
        {
            "myocardium of the left ventricle",
            "left atrium blood cavity",
            "left ventricle blood cavity",
            "right atrium blood cavity",
            "right ventricle blood cavity",
            "pulmonary artery",
            "ascending aorta"
        };

        private float _intensityCorrection = 45.0f;

        ///

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            _texLoading = Texture.LoadFromFile("Data/loading.png");
            _shaderText = new Shader("Shaders/vshader_text.glsl", "Shaders/fshader_text.glsl");
            TextImage textImageLoading = new TextImage();
            textImageLoading.ConstructVAO();
            textImageLoading.BindToShader(_shaderText);
            textImageLoading.transform = Matrix4.CreateScale(0.5f);

            // start with blank screen
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shaderText.Use();
            _texLoading.Use(TextureUnit.Texture1);
            _shaderText.SetMatrix4("model", textImageLoading.transform);
            textImageLoading.Draw();
            SwapBuffers();
            textImageLoading.Delete();

            // load textures
            this.Title = string.Format("Medical Viewer - LOADING ASSETS - TEXTURE");
            _tex3D = Texture3D.LoadFromFile("Data/CT/ct_image.nii.gz");
            _texInstructions = Texture.LoadFromFile("Data/instructions.png");

            // compile shaders
            this.Title = string.Format("Medical Viewer - LOADING ASSETS - SHADERS");
            _shader = new Shader("Shaders/vshader.glsl", "Shaders/fshader.glsl");
            _shaderPlane = new Shader("Shaders/vshader_plane.glsl", "Shaders/fshader_plane.glsl");
            _shaderWidget = new Shader("Shaders/vshader_widget.glsl", "Shaders/fshader_widget.glsl");

            // set camera
            Vector3 camPos = new Vector3
            {
                X = 1.3f,
                Y = 0.75f,
                Z = 1.3f
            };
            _camera = new Camera(camPos, Size.X / (float)Size.Y);
            _camera.Yaw = -136f;
            _camera.Pitch = -10.0f;

            // load anatomical models
            // load sform or qform
            Matrix4 modelTransform = _tex3D.Transformation;
            modelTransform.Transpose();
            // transform to world coordinates
            modelTransform.Invert();
            // normalize coordinates within 0-1 range
            modelTransform *= Matrix4.CreateScale(
                                1.0f / _tex3D.Dimensions.X,
                                1.0f / _tex3D.Dimensions.Y,
                                1.0f / _tex3D.Dimensions.Z);

            Random rand = new Random();
            int objectCounter = 0;
            foreach (string f in modelFiles)
            {
                Model m = Model.Load(f);
                m.ConstructVAO();
                m.BindToShader(_shader);
                m.transform = modelTransform;
                m.color = new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());  
                m.name = _modelTitles[objectCounter];
                m.objectID = ++objectCounter * 10.0f;
                m.CalculateCenterOfMass();
                _models.Add(m);

                this.Title = string.Format("Medical Viewer - LOADING ASSETS - MODEL ({0}/{1})", objectCounter, modelFiles.Length);
            }

            // create anatomical planes
            var plane_x = Model.Load("Data/plane.obj");
            plane_x.ConstructVAO();
            plane_x.BindToShader(_shaderPlane);
            plane_x.name = "Plane_X";
            plane_x.visible = true;
            plane_x.objectID = ++objectCounter * 10.0f;
            _planes.Add(plane_x);

            var plane_y = Model.Load("Data/plane.obj");
            plane_y.ConstructVAO();
            plane_y.BindToShader(_shaderPlane);
            plane_y.transform = Matrix4.CreateRotationY(-(float)Math.PI*0.5f);
            plane_y.name = "Plane_Y";
            plane_y.visible = false;
            plane_y.objectID = ++objectCounter * 10.0f;
            _planes.Add(plane_y);

            var plane_z = Model.Load("Data/plane.obj");
            plane_z.ConstructVAO();
            plane_z.BindToShader(_shaderPlane);
            plane_z.transform = Matrix4.CreateRotationX((float)Math.PI * 0.5f);
            plane_z.name = "Plane_Z";
            plane_z.visible = false;
            plane_z.objectID = ++objectCounter * 10.0f;
            _planes.Add(plane_z);

            // create widgets
            float widgetScale = 0.65f;

            var rotation_widget_x = Model.Load("Data/rotation_widget.obj");
            rotation_widget_x.ConstructVAO();
            rotation_widget_x.BindToShader(_shaderWidget);
            rotation_widget_x.color = new Vector3(1.0f, 0.0f, 0.0f);
            rotation_widget_x.CalculateCenterOfMass();
            rotation_widget_x.transform = Matrix4.CreateScale(widgetScale);
            rotation_widget_x.transform *= Matrix4.CreateRotationY((float)Math.PI / 2.0f);
            rotation_widget_x.transform *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            rotation_widget_x.objectID = ++objectCounter * 10.0f;
            rotation_widget_x.name = "Rotation_Widget_X";
            _widgets.Add(rotation_widget_x);

            var rotation_widget_y = Model.Load("Data/rotation_widget.obj");
            rotation_widget_y.ConstructVAO();
            rotation_widget_y.BindToShader(_shaderWidget);
            rotation_widget_y.color = new Vector3(0.0f, 1.0f, 0.0f);
            rotation_widget_y.CalculateCenterOfMass();
            rotation_widget_y.transform = Matrix4.CreateScale(widgetScale);
            rotation_widget_y.transform *= Matrix4.CreateRotationX((float)Math.PI / 2.0f);
            rotation_widget_y.transform *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            rotation_widget_y.objectID = ++objectCounter * 10.0f;
            rotation_widget_y.name = "Rotation_Widget_Y";
            _widgets.Add(rotation_widget_y);

            var rotation_widget_z = Model.Load("Data/rotation_widget.obj");
            rotation_widget_z.ConstructVAO();
            rotation_widget_z.BindToShader(_shaderWidget);
            rotation_widget_z.color = new Vector3(0.0f, 0.0f, 1.0f);
            rotation_widget_z.CalculateCenterOfMass();
            rotation_widget_z.transform = Matrix4.CreateScale(widgetScale);
            rotation_widget_z.transform *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            rotation_widget_z.objectID = ++objectCounter * 10.0f;
            rotation_widget_z.name = "Rotation_Widget_Z";
            _widgets.Add(rotation_widget_z);

            // instructions text
            _instructionsText = new TextImage();
            _instructionsText.ConstructVAO();
            _instructionsText.BindToShader(_shaderText);
            _instructionsText.transform = Matrix4.CreateScale(0.5f);
            _instructionsText.transform *= Matrix4.CreateTranslation(-0.73f, 0.82f, 0.0f);

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

            float time = (float)(_sw.ElapsedMilliseconds) / 1000.0f;

            //
            //  calculate transforms
            //

            foreach (Model m in _models)
            {
                Vector4 centroidTrans = new Vector4(m.centerOfMass);
                Matrix4 mm = m.transform;
                if(_bBreathing)
                {
                    mm *= Matrix4.CreateTranslation(-centroidTrans.X, -centroidTrans.Y, -centroidTrans.Z);
                    mm *= Matrix4.CreateScale((float)(1.0 - 0.05 * (0.5 + 0.5 * Math.Sin(time * 0.8))));
                    mm *= Matrix4.CreateTranslation(centroidTrans.X, centroidTrans.Y, centroidTrans.Z);
                }

                Matrix4 mm2 = Matrix4.Identity;
                mm2 *= Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f);
                mm2 *= Matrix4.CreateRotationX(_xRot);
                mm2 *= Matrix4.CreateRotationY(_yRot);
                mm2 *= Matrix4.CreateRotationZ(_zRot);
                mm2 *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);

                m.frame_transform = mm;
                m.frame_transform2 = mm2;
            }

            foreach (Model m in _planes)
            {
                Matrix4 mm = m.transform;
                if (m.name == "Plane_X")
                    mm *= Matrix4.CreateTranslation(0.0f, 0.0f, _xPlaneTrans);
                else if (m.name == "Plane_Y")
                    mm *= Matrix4.CreateTranslation(_yPlaneTrans, 0.0f, 0.0f);
                else if (m.name == "Plane_Z")
                    mm *= Matrix4.CreateTranslation(0.0f, _zPlaneTrans, 0.0f);

                Matrix4 mm2 = Matrix4.Identity;
                mm2 *= Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f);
                mm2 *= Matrix4.CreateRotationX(_xRot);
                mm2 *= Matrix4.CreateRotationY(_yRot);
                mm2 *= Matrix4.CreateRotationZ(_zRot);
                mm2 *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);

                m.frame_transform = mm;
                m.frame_transform2 = mm2;
            }

            foreach (Model m in _widgets)
            {
                Matrix4 mm = m.transform;
                mm *= Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f);
                if (m.name == "Rotation_Widget_X")
                    mm *= Matrix4.CreateRotationX(_xRot);
                else if (m.name == "Rotation_Widget_Y")
                    mm *= Matrix4.CreateRotationY(_yRot);
                else if (m.name == "Rotation_Widget_Z")
                    mm *= Matrix4.CreateRotationZ(_zRot);
                mm *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);

                m.frame_transform = mm;
            }

            //
            //  object id map
            //

            _shader.Use();
            _shader.SetInt("renderMode", (int)RenderMode.ObjectID);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            foreach (Model m in _models)
            {
                _shader.SetMatrix4("model", m.frame_transform);
                _shader.SetMatrix4("model2", m.frame_transform2);
                _shader.SetFloat("objectID", m.objectID / 255.0f);
                m.Draw();
            }

            _shaderPlane.Use();
            _shaderPlane.SetInt("renderMode", (int)RenderMode.ObjectID);
            _shaderPlane.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderPlane.SetMatrix4("projection", _camera.GetProjectionMatrix());
            GL.Disable(EnableCap.CullFace);
            foreach (Model m in _planes)
            {
                _shaderPlane.SetMatrix4("model", m.frame_transform);
                _shaderPlane.SetMatrix4("model2", m.frame_transform2);
                _shaderPlane.SetFloat("objectID", m.objectID / 255.0f);
                m.Draw();
            }
            GL.Enable(EnableCap.CullFace);

            _shaderWidget.Use();
            _shaderWidget.SetInt("renderMode", (int)RenderMode.ObjectID);
            _shaderWidget.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderWidget.SetMatrix4("projection", _camera.GetProjectionMatrix());
            foreach (Model m in _widgets)
            {
                _shaderWidget.SetMatrix4("model", m.transform);
                _shaderWidget.SetFloat("objectID", m.objectID / 255.0f);
                m.Draw();
            }

            IntPtr Pixel = new IntPtr();
            GL.ReadPixels((int)(MousePosition.X), (int)(Size.Y - MousePosition.Y), 1, 1, PixelFormat.Red, PixelType.UnsignedByte, ref Pixel);
            cval = (int)Pixel;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            //
            //  instructions
            //

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            _shaderText.Use();
            _texInstructions.Use(TextureUnit.Texture1);
            _shaderText.SetMatrix4("model", _instructionsText.transform);
            _instructionsText.Draw();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

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
            _shader.SetFloat("norm", _intensityCorrection);
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
            //  anatomical planes
            //

            _shaderPlane.Use();
            _shaderPlane.SetInt("renderMode", (int)RenderMode.Regular);
            _shaderPlane.SetFloat("norm", _intensityCorrection);

            GL.Disable(EnableCap.CullFace);
            foreach (Model m in _planes)
            {
                _shaderPlane.SetMatrix4("model", m.frame_transform);
                _shaderPlane.SetMatrix4("model2", m.frame_transform2);
                m.Draw();
            }
            GL.Enable(EnableCap.CullFace);


            //
            //  coordinate system
            //

            _shaderWidget.Use();
            _shaderWidget.SetInt("renderMode", (int)RenderMode.Regular);
            _shaderWidget.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderWidget.SetMatrix4("projection", _camera.GetProjectionMatrix());
            foreach (Model m in _widgets)
            {
                _shaderWidget.SetMatrix4("model", m.frame_transform);
                if(m.isSelected || m.isHovering)
                    _shaderWidget.SetVector3("color", new Vector3(1.0f, 1.0f, 0.0f));
                else
                    _shaderWidget.SetVector3("color", m.color);
                m.Draw();
            }


            //
            //  selected object outline
            //

            foreach (Model m in _models)
            {
                if (!(m.isSelected || m.isHovering))
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
                if(m.isSelected)
                    _shader.SetVector3("outlineColor", new Vector3(1.0f, 1.0f, 0.0f));
                else
                    _shader.SetVector3("outlineColor", new Vector3(1.0f, 0.0f, 0.0f));
                m.Draw();

                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilMask(0x00);
                GL.Enable(EnableCap.DepthTest);
            }

            GL.Disable(EnableCap.CullFace);
            foreach (Model m in _planes)
            {
                if (!(m.isSelected || m.isHovering))
                    continue;

                _shaderPlane.Use();

                // disable writing to color space
                GL.ColorMask(false, false, false, false);
                GL.DepthMask(false);

                GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilMask(0xFF); // enable writing to stencil
                GL.Disable(EnableCap.DepthTest);

                _shaderPlane.SetInt("renderMode", (int)RenderMode.Regular);
                _shaderPlane.SetMatrix4("model", m.frame_transform);
                _shaderPlane.SetMatrix4("model2", m.frame_transform2);
                m.Draw();

                GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF); // only allow fragments outside the stencil
                GL.StencilMask(0x00); // disable writing to stencil

                // enable writing to color space
                GL.ColorMask(true, true, true, true);
                GL.DepthMask(true);

                _shaderPlane.SetInt("renderMode", (int)RenderMode.Outline);
                if (m.isSelected)
                    _shaderPlane.SetVector3("outlineColor", new Vector3(1.0f, 1.0f, 0.0f));
                else
                    _shaderPlane.SetVector3("outlineColor", new Vector3(1.0f, 0.0f, 0.0f));
                Matrix4 mm2 = m.frame_transform;
                mm2 *= Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f);
                mm2 *= Matrix4.CreateScale(1.02f, 1.02f, 1.0f);
                mm2 *= Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
                _shaderPlane.SetMatrix4("model", mm2);
                m.Draw();

                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilMask(0x00);
                GL.Enable(EnableCap.DepthTest);
            }
            GL.Enable(EnableCap.CullFace);

            // finish
            SwapBuffers();
            GL.BindVertexArray(0);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // calculate time passed
            long newTime = _sw.ElapsedMilliseconds;
            long diff = newTime - _prevElapsedTime;

            // update window title and fps
            string objectTitle = "";
            foreach (Model m in _models)
            {
                if (m.isSelected)
                    objectTitle = m.name;
            }
            if (_prevElapsedTime != 0)
                this.Title = string.Format("Medical Viewer - frametime: {0} ms ({1} FPS) {2}", diff, Math.Round(_previousFPS.Average()), objectTitle.Length > 0 ? "- " + objectTitle : "");
            _prevElapsedTime = newTime;

            if(diff > 0)
            {
                _previousFPS[_previousFPSIndex] = 1000.0f / diff;
                _previousFPSIndex++;
                if (_previousFPSIndex >= 100)
                    _previousFPSIndex = 0;
            }

            // dont respond to any input if the window is not focussed
            if (!IsFocused)
            {
                return;
            }

            // press escape to close window
            var input = KeyboardState;
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if (input.IsKeyPressed(Keys.W))
            {
                _bBreathing = !_bBreathing;
            }
            if (input.IsKeyPressed(Keys.Q))
            {
                _bSpinning = !_bSpinning;
            }
            if (input.IsKeyPressed(Keys.E))
            {
                foreach (Model m in _models)
                {
                    m.visible = !m.visible;
                }
            }
            if (input.IsKeyPressed(Keys.D1))
            {
                _planes[0].visible = !_planes[0].visible;
            }
            if (input.IsKeyPressed(Keys.D2))
            {
                _planes[1].visible = !_planes[1].visible;
            }
            if (input.IsKeyPressed(Keys.D3))
            {
                _planes[2].visible = !_planes[2].visible;
            }

            //
            //  set hovering and object selection
            //

            var mouse = MouseState;
            foreach (Model m in _models)
            {
                if (m.objectID == (float)cval)
                    m.isHovering = true;
                else
                    m.isHovering = false;

                if (!_hasActedOnClick && m.isHovering && mouse.IsAnyButtonDown)
                {
                    m.isSelected = true;
                    _hasActedOnClick = true;
                    foreach (Model m2 in _models)
                    {
                        if (m.objectID != m2.objectID)
                            m2.isSelected = false;
                    }
                }
            }

            foreach (Model m in _planes)
            {
                if (m.objectID == (float)cval)
                    m.isHovering = true;
                else
                    m.isHovering = false;

                if (!_hasActedOnClick && m.isHovering && mouse.IsAnyButtonDown)
                {
                    m.isSelected = true;
                    _hasActedOnClick = true;
                    foreach (Model m2 in _planes)
                    {
                        if (m.objectID != m2.objectID)
                            m2.isSelected = false;
                    }
                }
                else if (!mouse.IsAnyButtonDown)
                {
                    m.isSelected = false;
                }
            }

            foreach (Model m in _widgets)
            {
                if (m.objectID == (float)cval)
                    m.isHovering = true;
                else
                    m.isHovering = false;

                if (!_hasActedOnClick && m.isHovering && mouse.IsAnyButtonDown)
                {
                    m.isSelected = true;
                    _hasActedOnClick = true;
                    foreach (Model m2 in _widgets)
                    {
                        if (m.objectID != m2.objectID)
                            m2.isSelected = false;
                    }
                }
                else if(!mouse.IsAnyButtonDown)
                {
                    m.isSelected = false;
                }
            }

            // deselect everything when background clicked
            if(!_hasActedOnClick && mouse.IsAnyButtonDown)
            {
                if (cval == 255)
                {
                    foreach (Model m in _models)
                    {
                        m.isSelected = false;
                    }
                    foreach (Model m in _widgets)
                    {
                        m.isSelected = false;
                    }
                }
            }

            bool anyWidgetSelected = false;
            foreach (Model m in _widgets)
            {
                if (m.isSelected)
                    anyWidgetSelected = true;
            }
            if (_bSpinning && (!mouse.IsAnyButtonDown || !anyWidgetSelected))
                _yRot += diff * 0.001f;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
                _lastMouseMovedTime = newTime;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                if(mouse.IsAnyButtonDown)
                {
                    foreach (Model m in _widgets)
                    {
                        if (m.isSelected)
                        {
                            if (m.name == "Rotation_Widget_X")
                            {
                                _xRot += deltaX * 0.005f;
                            }
                            else if (m.name == "Rotation_Widget_Y")
                            {
                                _yRot += deltaX * 0.005f;
                            }
                            else if (m.name == "Rotation_Widget_Z")
                            {
                                _zRot += deltaX * 0.005f;
                            }
                        }
                    }
                    foreach (Model m in _planes)
                    {
                        if (m.isSelected)
                        {
                            if (m.name == "Plane_X")
                            {
                                _xPlaneTrans += deltaX * 0.002f;
                                _xPlaneTrans = Math.Clamp(_xPlaneTrans, 0.0f, 1.0f);
                            }
                            else if (m.name == "Plane_Y")
                            {
                                _yPlaneTrans += deltaX * 0.002f;
                                _yPlaneTrans = Math.Clamp(_yPlaneTrans, 0.0f, 1.0f);
                            }
                            else if (m.name == "Plane_Z")
                            {
                                _zPlaneTrans += deltaX * 0.002f;
                                _zPlaneTrans = Math.Clamp(_zPlaneTrans, 0.0f, 1.0f);
                            }
                        }
                    }
                }

                if(mouse.X >= 0 && mouse.X < Size.X &&
                   mouse.Y >= 0 && mouse.Y < Size.Y &&
                   (deltaX > 0 || deltaY > 0))
                {
                    _lastMouseMovedTime = newTime;
                    foreach (Model m in _widgets)
                    {
                        m.visible = true;
                    }
                }
            }

            if (!mouse.IsAnyButtonDown)
                _hasActedOnClick = false;

            // hide rotation widgets after some time
            if (!anyWidgetSelected && newTime - _lastMouseMovedTime > 2000)
            {
                foreach (Model m in _widgets)
                {
                    m.visible = false;
                }
            }
            else if(anyWidgetSelected)
            {
                _lastMouseMovedTime = newTime;
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
            foreach (Model m in _widgets)
            {
                m.Delete();
            }
            _instructionsText.Delete();

            GL.DeleteProgram(_shader.Handle);
            GL.DeleteProgram(_shaderPlane.Handle);

            base.OnUnload();
        }
    }
}
