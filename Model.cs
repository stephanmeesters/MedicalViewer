using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace LearnOpenTK
{
    class Model
    {
        public readonly Mesh mesh;

        public int vao;
        public int vbo_vertex;
        public int vbo_index;

        public Matrix4 transform = Matrix4.Identity;

        public Vector3 color = new Vector3(1.0f, 1.0f, 1.0f);

        public PrimitiveType renderType = PrimitiveType.Triangles;

        public string name = "";
        public bool visible = true;
        public float objectID;

        public Vector3 centerOfMass = new Vector3();

        public Matrix4 frame_transform;
        public Matrix4 frame_transform2;

        public Model(Mesh mesh)
        {
            this.mesh = mesh;
        }

        public static Model Load(string path)
        {
            var mesh = ObjLoader.Load(path);
            return new Model(mesh);
        }

        public void ConstructVAO()
        {
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vbo_vertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vertex);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.numberOfAttributes * sizeof(float), mesh.vertices_attributes, BufferUsageHint.StaticDraw);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            vbo_index = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_index);
            GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.numberOfIndices * sizeof(uint), mesh.vertexIndices.ToArray(), BufferUsageHint.StaticDraw);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void BindToShader(Shader shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vertex);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_index);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            var positionLocation = shader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, mesh.stride * sizeof(float), 0);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            var normalLocation = shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, true, mesh.stride * sizeof(float), 3 * sizeof(float));
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Use()
        {
            
        }

        public void Draw()
        {
            if (!visible)
                return;

            GL.BindVertexArray(vao);
            GL.DrawElements(renderType, mesh.numberOfIndices, DrawElementsType.UnsignedInt, 0);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);
            GL.BindVertexArray(0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(vbo_vertex);
            GL.DeleteBuffer(vbo_index);
            GL.DeleteVertexArray(vao);
        }

        public void CalculateCenterOfMass()
        {
            /*Vector3 centerOfMass = new Vector3();
            for (int i = 0; i < this.mesh.numberOfVertices; i++)
            {
                Vector4 m = new Vector4(this.mesh.vertices[i], 1.0f);
                m *= this.transform;
                centerOfMass += new Vector3(m.X, m.Y, m.Z);
            }
            centerOfMass /= this.mesh.numberOfVertices;
            this.centerOfMass = centerOfMass;*/

            Vector4 m = new Vector4(this.mesh.centerOfMass, 1.0f);
            m *= this.transform;
            this.centerOfMass = new Vector3(m.X, m.Y, m.Z);
        }
    }
}
