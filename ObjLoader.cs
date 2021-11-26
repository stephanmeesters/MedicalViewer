using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;

namespace LearnOpenTK
{
	public static class ObjLoader
	{
		public static Mesh Load(string path)
		{
			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<uint> vertexIndices = new List<uint>();
			List<uint> normalIndices = new List<uint>();

			if (!File.Exists(path))
			{
				throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
			}

			using (StreamReader streamReader = new StreamReader(path))
			{
				while (!streamReader.EndOfStream)
				{
					List<string> words = new List<string>(streamReader.ReadLine().ToLower().Split(' '));
					words.RemoveAll(s => s == string.Empty);

					if (words.Count == 0)
						continue;

					string type = words[0];
					words.RemoveAt(0);

					

					switch (type)
					{
						// vertex
						case "v":
							vertices.Add(new Vector3(
								float.Parse(words[0], System.Globalization.CultureInfo.InvariantCulture),
								float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture),
								float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture)));
							break;

						case "vn":
							normals.Add(new Vector3(
								float.Parse(words[0], System.Globalization.CultureInfo.InvariantCulture), 
								float.Parse(words[1], System.Globalization.CultureInfo.InvariantCulture), 
								float.Parse(words[2], System.Globalization.CultureInfo.InvariantCulture)));
							break;

						// face
						case "f":
							foreach (string w in words)
							{
								if (w.Length == 0)
									continue;

								string[] comps = w.Split('/');

								// subtract 1: indices start from 1, not 0
								vertexIndices.Add(uint.Parse(comps[0]) - 1);

								if (comps.Length > 2)
									normalIndices.Add(uint.Parse(comps[2]) - 1);
							}
							break;

						default:
							break;
					}
				}
			}

			return new Mesh(vertices, normals, vertexIndices, normalIndices);
		}
	}
}