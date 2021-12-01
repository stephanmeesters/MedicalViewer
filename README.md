# MedicalViewer

Medical visualization demonstration app.

Written in C#/.NET Core 3.1 using OpenTK.

Built in ~40 hours as part of my software programming portfolio.

Several classes (Camera.cs, Shader.cs, Texture.cs) adapted from [LearnOpenTK](https://github.com/opentk/LearnOpenTK) and (ObjLoader.cs) from [ObjRenderer](https://github.com/dabbertorres/ObjRenderer).

Uses labelled CT data from [MM-WHS: Multi-Modality Whole Heart Segmentation 2017](http://www.sdspeople.fudan.edu.cn/zhuangxiahai/0/mmwhs).
The segmentations were converted into .obj meshes using ITKSnap and were subsequently cleaned up using MeshLab (reducing polygon count, fixing holes).

![Screenshot](https://github.com/stephanmeesters/MedicalViewer/blob/main/screenshot.png?raw=true)
