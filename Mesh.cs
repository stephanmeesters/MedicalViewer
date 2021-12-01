using System.Collections.Generic;
using OpenTK.Mathematics;

namespace MedViewer
{
	public class Mesh
	{
		public readonly float[] vertices_attributes;
		public readonly List<Vector3> vertices;
		public readonly List<Vector3> normals;
		public readonly List<uint> vertexIndices;
		public readonly List<uint> normalIndices;

		public readonly Vector3 centerOfMass;

		public readonly int numberOfVertices;
		public readonly int numberOfIndices;
		public readonly int numberOfAttributes;
		public readonly int stride;

		public Mesh(List<Vector3> vertices, List<Vector3> normals,
					List<uint> vertexIndices, List<uint> normalIndices)
		{
			this.vertices = vertices;
			this.normals = normals;
			this.vertexIndices = vertexIndices;
			this.normalIndices = normalIndices;

			this.numberOfVertices = vertices.Count;
			this.numberOfIndices = vertexIndices.Count;
			this.stride = 6;
			this.numberOfAttributes = this.numberOfVertices * this.stride;

			// per vertex: 3 coordinates, and 3 normal coordinates
			this.vertices_attributes = new float[this.numberOfAttributes];
			for(int i = 0; i< this.numberOfVertices; i++)
            {
				this.vertices_attributes[i * this.stride + 0] = this.vertices[i].X;
				this.vertices_attributes[i * this.stride + 1] = this.vertices[i].Y;
                this.vertices_attributes[i * this.stride + 2] = this.vertices[i].Z;

                centerOfMass += this.vertices[i];
			}
			centerOfMass /= this.numberOfVertices;

			Vector3[] normals_averaged = new Vector3[this.numberOfVertices];
			int[] normals_averaged_count = new int[this.numberOfVertices];

			for (int j = 0; j < this.numberOfIndices; j++)
			{
				int i = (int)vertexIndices[j];
				normals_averaged[i] += this.normals[(int)this.normalIndices[j]];
				normals_averaged_count[i]++;
				
			}
			for (int i = 0; i < this.numberOfVertices; i++)
			{
				normals_averaged[i].Normalize();
				this.vertices_attributes[i * this.stride + 3] = normals_averaged[i].X;
				this.vertices_attributes[i * this.stride + 4] = normals_averaged[i].Y;
				this.vertices_attributes[i * this.stride + 5] = normals_averaged[i].Z;
			}

		}
	}
}