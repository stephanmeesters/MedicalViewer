using System.Collections.Generic;
using OpenTK.Mathematics;

namespace LearnOpenTK
{
	public class Mesh
	{
		public readonly List<float> vertices;
		public readonly List<Vector3> normals;
		public readonly List<uint> vertexIndices;
		public readonly List<uint> normalIndices;

		public readonly int numberOfVertices;
		public readonly int numberOfIndices;

		public readonly BoundingBox boundingBox;

		public Mesh(List<float> vertices, List<Vector3> normals,
					List<uint> vertexIndices, List<uint> normalIndices)
		{
			this.vertices = vertices;
			this.normals = normals;
			this.vertexIndices = vertexIndices;
			this.normalIndices = normalIndices;

			this.numberOfVertices = vertices.Count;
			this.numberOfIndices = vertexIndices.Count;
/*
			Vector3 min = vertices[0];
			Vector3 max = vertices[0];

			foreach (Vector3 v in vertices)
			{
				if (v.X <= min.X)
				{
					min.X = v.X;
				}
				else if (v.X >= max.X)
				{
					max.X = v.X;
				}

				if (v.Y <= min.Y)
				{
					min.Y = v.Y;
				}
				else if (v.Y >= max.Y)
				{
					max.Y = v.Y;
				}

				if (v.Z <= min.Z)
				{
					min.Z = v.Z;
				}
				else if (v.Z >= max.Z)
				{
					max.Z = v.Z;
				}
			}

			boundingBox = new BoundingBox(min, max);*/

			
		}
	}
}