# MedicalViewer

Medical visualization demonstration app.

Written in C#/.NET Core 3.1 using OpenTK.

Uses 3D textures for interactive slicing within the OpenGL shaders.

Built in ~40 hours as part of my software programming portfolio.

Several classes (Camera.cs, Shader.cs, Texture.cs) adapted and modified from ["LearnOpenTK"](https://github.com/opentk/LearnOpenTK) by [OpenTK](https://github.com/opentk) licensed under [CC 4.0](https://github.com/opentk/LearnOpenTK/blob/master/LICENSE) and one class (ObjLoader.cs) adapted and modified from ["ObjRenderer"](https://github.com/dabbertorres/ObjRenderer) by [dabbertorres](https://github.com/dabbertorres) licensed under [MIT](https://github.com/dabbertorres/ObjRenderer/blob/master/LICENSE).

Uses labelled CT data from [MM-WHS: Multi-Modality Whole Heart Segmentation 2017](http://www.sdspeople.fudan.edu.cn/zhuangxiahai/0/mmwhs).
The segmentations were converted into .obj meshes using ITKSnap and were subsequently cleaned up using MeshLab (reducing polygon count, fixing holes).

![Screenshot](https://github.com/stephanmeesters/MedicalViewer/blob/main/screenshot.png?raw=true)
