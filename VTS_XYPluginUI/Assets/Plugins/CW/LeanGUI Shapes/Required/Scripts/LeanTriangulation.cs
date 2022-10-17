using UnityEngine;
using System.Collections.Generic;

namespace Lean.Gui
{
	/// <summary>This class calculates a triangulation for a given list of vertices.
	/// This is used by the <b>LeanPolygon</b> component.</summary>
	public static class LeanTriangulation
	{
		public class Vertex
		{
			public int     I;
			public Vector2 XY;
		}

		public class Triangle
		{
			public int IndexA;
			public int IndexB;
			public int IndexC;
		}

		public static List<Triangle> Triangles = new List<Triangle>();

		public static List<Vector2> Points = new List<Vector2>();

		private static List<Vertex> vertices = new List<Vertex>();

		public static bool Calculate(List<Vector2> coords)
		{
			vertices.Clear();
			Triangles.Clear();
			Points.Clear();

			if (coords != null && coords.Count > 2)
			{
				for (var i = 0; i < coords.Count; i++)
				{
					var vertex = new Vertex(); vertices.Add(vertex);
					var coord  = coords[i];

					vertex.I  = i;
					vertex.XY = coord;

					Points.Add(coord);
				}

				vertices.Add(vertices[0]);

				for (var i = coords.Count - 4; i >= 0; i--)
				{
					Clip();
				}

				Submit(vertices[0].I, vertices[1].I, vertices[2].I);

				return true;
			}

			return false;
		}

		private static void Clip()
		{
			for (var i = vertices.Count - 1; i >= 2; i--)
			{
				var vertexA = vertices[i - 2];
				var vertexB = vertices[i - 1];
				var vertexC = vertices[i    ];

				if (CanClip(vertexA.XY, vertexB.XY, vertexC.XY, i - 2, i) == true)
				{
					Submit(vertexA.I, vertexB.I, vertexC.I);

					vertices.RemoveAt(i - 1);

					return;
				}
			}
		}

		private static bool CanClip(Vector2 pointA, Vector2 pointB, Vector2 pointC, int min, int max)
		{
			if (LineSide(pointA, pointB, pointC) <= 0.0f)
			{
				return false;
			}

			for (var i = vertices.Count - 1; i >= 1; i--)
			{
				if (i < min || i > max)
				{
					if (PointInTriangle(pointA, pointB, pointC, vertices[i].XY) == true)
					{
						return false;
					}
				}
			}

			return true;
		}

		private static void Submit(int indexA, int indexB, int indexC)
		{
			var triangle = new Triangle(); Triangles.Add(triangle);

			triangle.IndexA = indexA;
			triangle.IndexB = indexB;
			triangle.IndexC = indexC;
		}

		private static float PointInSphere(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec)
		{
			float a = aVec.x - dVec.x;
			float d = bVec.x - dVec.x;
			float g = cVec.x - dVec.x;

			float b = aVec.y - dVec.y;
			float e = bVec.y - dVec.y;
			float h = cVec.y - dVec.y;

			float c = a * a + b * b;
			float f = d * d + e * e;
			float i = g * g + h * h;

			return (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);
		}

		private static double LineSide(Vector2 a, Vector2 b, Vector2 p)
		{
			return (b.y - a.y) * (p.x - a.x) - (b.x - a.x) * (p.y - a.y);
		}

		private static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p) // NOTE: CW
		{
			if (LineSide(a, b, p) >= 0.0f && LineSide(b, c, p) >= 0.0f && LineSide(c, a, p) >= 0.0f)
			{
				return true;
			}

			return false;
		}
	}
}