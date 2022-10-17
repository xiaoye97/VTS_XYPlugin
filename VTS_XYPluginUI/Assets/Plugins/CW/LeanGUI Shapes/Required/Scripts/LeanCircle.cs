using UnityEngine;
using UnityEngine.UI;
using Lean.Common;
using CW.Common;
using System.Collections.Generic;

namespace Lean.Gui
{
	/// <summary>This component allows you to create UI elements with a circle shape.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasRenderer))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanCircle")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Circle")]
	public class LeanCircle : MaskableGraphic
	{
		/// <summary>This allows you to set the blur radius in local space.
		/// 0 = No Blur.</summary>
		public float Blur { set { if (blur != value) { blur = value; SetAllDirty(); } } get { return blur; } } [SerializeField] private float blur = 0.5f;

		/// <summary>This allows you to set the thickness of the border in local space.
		/// -1 = Filled.</summary>
		public float Thickness { set { if (thickness != value) { thickness = value; SetAllDirty(); } } get { return thickness; } } [SerializeField] private float thickness = -1.0f;

		/// <summary>This allows you to set the amount of points used to draw a full sphere outer edge.</summary>
		public int Detail { set { if (detail != value) { detail = value; SetAllDirty(); } } get { return detail; } } [SerializeField] private int detail = 10;

		/// <summary>This allows you to control where the triangle begins in degrees.
		/// 0 = Top.
		/// 90 = Right.</summary>
		public float Angle { set { if (angle != value) { angle = value; SetAllDirty(); } } get { return angle; } } [SerializeField] private float angle;

		/// <summary>This allows you to control how much the circle is radially filled.
		/// 0.25 = Quarter Filled Clockwise.
		/// -0.5 = Half Filled Counter-Clockwise.
		/// 1.0 = Fully Filled.
		/// -1.5 = Filled 1.5 Times Counter-Clockwise.</summary>
		public float Fill { set { if (fill != value) { fill = value; SetAllDirty(); } } get { return fill; } } [SerializeField] private float fill = 1.0f;

		private static Texture2D blurTexture;
		private static bool      blurTextureSet;

		private static UIVertex innerVert = UIVertex.simpleVert;
		private static UIVertex outerVert = UIVertex.simpleVert;

		private static VertexHelper vh;

		private static Vector2 size;
		private static Vector2 center;
		private static Vector2 direction;
		private static float   magnitude;

		private static List<int> headIndices = new List<int>();
		private static List<int> tailIndices = new List<int>();
		private static List<int> edgeIndices = new List<int>();

		public override Texture mainTexture
		{
			get
			{
				return LeanGuiSprite.CachedTexture;
			}
		}

		public void SetDetail(float d)
		{
			Detail = (int)d;
		}

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			LeanGuiSprite.UpdateCache();

			var rect = rectTransform.rect;

			vh     = vertexHelper;
			size   = rect.size * 0.5f;
			center = rect.center;

			vh.Clear();

			headIndices.Clear();
			tailIndices.Clear();

			if (detail > 2)
			{
				innerVert.color = color;
				innerVert.uv0   = LeanGuiSprite.CachedSolid;

				outerVert.color = color;
				outerVert.uv0   = LeanGuiSprite.CachedClear;
			
				var angleCur = 0.0f;
				var angleMax = Mathf.Abs(Mathf.PI * 2.0f * fill);
				var angleInc = Mathf.PI * 2.0f / detail;
				var sign     = Mathf.Sign(fill);
				var index    = 0;

				if (thickness < 0.0f)
				{
					if (blur > 0.0f)
					{
						innerVert.position = center; vh.AddVert(innerVert);

						WriteLineA(angleCur);

						while (angleCur < angleMax)
						{
							var nextAngle = Mathf.Min(angleCur + angleInc, angleMax);

							WriteLineA(nextAngle * sign);

							AddTriangle(0, index + 1, index + 3);

							AddTriangle(index + 1, index + 2, index + 3);
							AddTriangle(index + 4, index + 3, index + 2);

							angleCur = nextAngle; index += 2;
						}

						if (fill % 1.0f != 0.0f)
						{
							headIndices.Add(2); headIndices.Add(1); headIndices.Add(0);
							tailIndices.Add(0); tailIndices.Add(vh.currentVertCount - 2); tailIndices.Add(vh.currentVertCount - 1);
						}
					}
					else
					{
						innerVert.position = center; vh.AddVert(innerVert);

						WriteLineB(angleCur);

						while (angleCur < angleMax)
						{
							var nextAngle = Mathf.Min(angleCur + angleInc, angleMax);

							WriteLineB(nextAngle * sign);

							AddTriangle(0, index + 1, index + 2);

							angleCur = nextAngle; index += 1;
						}
					}
				}
				else
				{
					if (blur > 0.0f)
					{
						WriteLineC(angleCur);

						while (angleCur < angleMax)
						{
							var nextAngle = Mathf.Min(angleCur + angleInc, angleMax);

							WriteLineC(nextAngle * sign);
						
							AddTriangle(index + 0, index + 1, index + 4);
							AddTriangle(index + 5, index + 4, index + 1);

							AddTriangle(index + 1, index + 2, index + 5);
							AddTriangle(index + 6, index + 5, index + 2);

							AddTriangle(index + 2, index + 3, index + 6);
							AddTriangle(index + 7, index + 6, index + 3);

							angleCur = nextAngle; index += 4;
						}

						if (fill % 1.0f != 0.0f)
						{
							headIndices.Add(3); headIndices.Add(2); headIndices.Add(1); headIndices.Add(0);
							tailIndices.Add(vh.currentVertCount - 4); tailIndices.Add(vh.currentVertCount - 3); tailIndices.Add(vh.currentVertCount - 2); tailIndices.Add(vh.currentVertCount - 1);
						}
					}
					else
					{
						WriteLineD(angleCur);

						while (angleCur < angleMax)
						{
							var nextAngle = Mathf.Min(angleCur + angleInc, angleMax);

							WriteLineD(nextAngle * sign);

							AddTriangle(index + 0, index + 1, index + 2);
							AddTriangle(index + 3, index + 2, index + 1);

							angleCur = nextAngle; index += 2;
						}
					}
				}

				WriteEdge(headIndices);
				WriteEdge(tailIndices);
			}
		}

		private void WriteEdge(List<int> indices)
		{
			if (indices.Count > 0)
			{
				edgeIndices.Clear();

				var vertA = default(UIVertex);
				var vertB = default(UIVertex);

				vh.PopulateUIVertex(ref vertA, indices[0]);
				vh.PopulateUIVertex(ref vertB, indices[1]);

				var vector = Vector2.Perpendicular(vertA.position - vertB.position).normalized * blur * 2.0f;

				if (fill < 0.0f)
				{
					vector = -vector;
				}

				for (var i = 0; i < indices.Count; i++)
				{
					var vert = default(UIVertex);

					vh.PopulateUIVertex(ref vert, indices[i]);

					vert.position += (Vector3)vector;
					vert.uv0       = LeanGuiSprite.CachedClear;

					edgeIndices.Add(vh.currentVertCount);

					vh.AddVert(vert);
				}

				for (var i = 0; i < indices.Count - 1; i++)
				{
					var a = indices[i];
					var b = indices[i + 1];
					var c = edgeIndices[i];
					var d = edgeIndices[i + 1];

					AddTriangle(a, b, c);
					AddTriangle(c, b, d);
				}
			}
		}

		private void Calc(float delta)
		{
			var final = angle * Mathf.Deg2Rad + delta;
			var point = new Vector2(Mathf.Sin(final) * size.x, Mathf.Cos(final) * size.y);

			direction = point.normalized;
			magnitude = point.magnitude;
		}

		private void AddTriangle(int a, int b, int c)
		{
			if (fill > 0.0f)
			{
				vh.AddTriangle(a, b, c);
			}
			else
			{
				vh.AddTriangle(a, c, b);
			}
		}

		private void WriteLineA(float delta)
		{
			Calc(delta);

			innerVert.position = center + direction * Mathf.Max(magnitude - blur, 0.0f); vh.AddVert(innerVert);
			outerVert.position = center + direction * (magnitude + blur); vh.AddVert(outerVert);
		}

		private void WriteLineB(float delta)
		{
			Calc(delta);

			innerVert.position = center + direction * magnitude; vh.AddVert(innerVert);
		}

		private void WriteLineC(float delta)
		{
			Calc(delta);

			var mid = Mathf.Max(0.0f, magnitude - thickness * 0.5f);
			
			outerVert.position = center + direction * Mathf.Clamp(magnitude - thickness - blur, 0.0f, mid); vh.AddVert(outerVert);
			innerVert.position = center + direction * Mathf.Clamp(magnitude - thickness + blur, 0.0f, mid); vh.AddVert(innerVert);

			innerVert.position = center + direction * Mathf.Max(magnitude - blur, mid); vh.AddVert(innerVert);
			outerVert.position = center + direction * (magnitude + blur); vh.AddVert(outerVert);
		}

		private void WriteLineD(float delta)
		{
			Calc(delta);

			var mid = Mathf.Max(0.0f, magnitude - thickness * 0.5f);
			
			innerVert.position = center + direction * Mathf.Clamp(magnitude - thickness, 0.0f, mid); vh.AddVert(innerVert);
			innerVert.position = center + direction * magnitude; vh.AddVert(innerVert);
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
	using UnityEditor;
	using TARGET = LeanCircle;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanCircle_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("m_Color", "This allows you to set the color of this element.");
			Draw("m_Material", "This allows you to specify a custom material for this element.");
			Draw("m_RaycastTarget", "Should UI pointers interact with this element?");
			Draw("blur", "This allows you to set the blur radius in local space.\n\n0 = No Blur.");
			Draw("thickness", "This allows you to set the thickness of the border in local space.\n\n-1 = Filled.");
			BeginError(Any(tgts, t => t.Detail < 3));
				Draw("detail", "This allows you to set the amount of points used to draw a full sphere outer edge.");
			EndError();
			Draw("angle", "This allows you to control where the triangle begins in degrees.\n\n0 = Top.\n\n90 = Right.");
			Draw("fill", "This allows you to control how much the circle is radially filled.\n\n0.25 = Quarter Filled Clockwise.\n\n-0.5 = Half Filled Counter-Clockwise.\n\n1.0 = Fully Filled.\n\n-1.5 = Filled 1.5 Times Counter-Clockwise.");
		}

		[MenuItem("GameObject/Lean/GUI/Circle", false, 1)]
		private static void CreateCircle()
		{
			Selection.activeObject = CwHelper.CreateElement<LeanCircle>(Selection.activeTransform);
		}
	}
}
#endif