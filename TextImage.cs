using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace LearnOpenTK
{
    class TextImage
    {
        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int vbo_index;
        private int vbo_vertex;
        private int vao;

        public Matrix4 transform = Matrix4.Identity;

        public void ConstructVAO()
        {
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vbo_vertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vertex);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            vbo_index = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_index);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint),_indices, BufferUsageHint.StaticDraw);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void BindToShader(Shader shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vertex);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_index);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            var vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Draw()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            Debug.Assert(GL.GetError() == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);
            GL.BindVertexArray(0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(vbo_vertex);
            GL.DeleteBuffer(vbo_index);
            GL.DeleteVertexArray(vao);
        }
    }
}
