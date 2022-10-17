using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Lean.Common;
using CW.Common;

namespace Lean.Gui
{
	/// <summary>This component allows you to create UI elements with a box shape.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasRenderer))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanBox")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Box")]
	public class LeanBox : MaskableGraphic
	{
		class Corner
		{
			public float Radius;

			public Vector2 DirectionA;

			public Vector2 Limits;

			public List<Vector2> Vectors = new List<Vector2>();

			public void Write(VertexHelper vh, float x, float y, float offset, bool limit)
			{
				var o = Radius;
				var r = Radius + offset;

				if (r < 0.0f)
				{
					o -= r;
					r = 0.0f;
				}

				/*
				if (Radius > limit)
				{
					limit = Radius;
				}

				if (o > limit)
				{
					b = o - limit;
					o = limit;
				}
				*/

				var delX = DirectionA.x * o;
				var delY = DirectionA.y * o;

				if (limit == true)
				{
					if (delY > Limits.y)
					{
						//delY = Limits.y;
					}
				}

				WriteRaw(vh, x, y, r, o, new Vector2(delX, delY));
			}

			public void WriteRaw(VertexHelper vh, float x, float y, float r, float o, Vector2 del)
			{
				for (var i = 0; i < Vectors.Count; i++)
				{
					var vector = Vectors[i];

					vector.x = vector.x * r + x - del.x;
					vector.y = vector.y * r + y - del.y;

					vert.position = vector;

					vh.AddVert(vert);
				}
			}
		}

		/// <summary>This allows you to set the blur radius in local space.</summary>
		public float Blur { set { if (blur != value) { blur = value; SetAllDirty(); } } get { return blur; } } [SerializeField] private float blur = 0.5f;

		/// <summary>This allows you to set the thickness of the border in local space.</summary>
		public float Thickness { set { if (thickness != value) { thickness = value; SetAllDirty(); } } get { return thickness; } } [SerializeField] private float thickness = -1.0f;

		/// <summary>This allows you to push out or pull in the edges of the box shape in local space.</summary>
		public float Inflate { set { if (inflate != value) { inflate = value; SetAllDirty(); } } get { return inflate; } } [SerializeField] private float inflate;

		/// <summary>This allows you to set the base detail of the box corners.</summary>
		public float Detail { set { if (detail != value) { detail = value; SetAllDirty(); } } get { return detail; } } [SerializeField] private float detail = 10.0f;

		/// <summary>This allows you to manually override the detail of the top left corner of this box.
		/// -1 = No Override</summary>
		public float DetailTL { set { if (detailTL != value) { detailTL = value; SetAllDirty(); } } get { return detailTL; } } [SerializeField] private float detailTL = -1.0f;

		/// <summary>This allows you to manually override the detail of the top right corner of this box.
		/// -1 = No Override</summary>
		public float DetailTR { set { if (detailTR != value) { detailTR = value; SetAllDirty(); } } get { return detailTR; } } [SerializeField] private float detailTR = -1.0f;

		/// <summary>This allows you to manually override the radius of the bottom left corner of this box.
		/// -1 = No Override</summary>
		public float DetailBL { set { if (detailBL != value) { detailBL = value; SetAllDirty(); } } get { return detailBL; } } [SerializeField] private float detailBL = -1.0f;

		/// <summary>This allows you to manually override the detail of the bottom right corner of this box.
		/// -1 = No Override</summary>
		public float DetailBR { set { if (detailBR != value) { detailBR = value; SetAllDirty(); } } get { return detailBR; } } [SerializeField] private float detailBR = -1.0f;

		/// <summary>This allows you to set the base radius of the box corners.</summary>
		public float Radius { set { if (radius != value) { radius = value; SetAllDirty(); } } get { return radius; } } [SerializeField] private float radius = 5.0f;

		/// <summary>This allows you to manually override the radius of the top left corner of this box.
		/// -1 = No Override</summary>
		public float RadiusTL { set { if (radiusTL != value) { radiusTL = value; SetAllDirty(); } } get { return radiusTL; } } [SerializeField] private float radiusTL = -1.0f;

		/// <summary>This allows you to manually override the radius of the top right corner of this box.
		/// -1 = No Override</summary>
		public float RadiusTR { set { if (radiusTR != value) { radiusTR = value; SetAllDirty(); } } get { return radiusTR; } } [SerializeField] private float radiusTR = -1.0f;

		/// <summary>This allows you to manually override the radius of the bottom left corner of this box.
		/// -1 = No Override</summary>
		public float RadiusBL { set { if (radiusBL != value) { radiusBL = value; SetAllDirty(); } } get { return radiusBL; } } [SerializeField] private float radiusBL = -1.0f;

		/// <summary>This allows you to manually override the radius of the bottom right corner of this box.
		/// -1 = No Override</summary>
		public float RadiusBR { set { if (radiusBR != value) { radiusBR = value; SetAllDirty(); } } get { return radiusBR; } } [SerializeField] private float radiusBR = -1.0f;

		private static UIVertex vert = UIVertex.simpleVert;

		private static int pointCount;

		private static Corner[] corners = new Corner[] { new Corner(), new Corner(), new Corner(), new Corner() };

		public override Texture mainTexture
		{
			get
			{
				return LeanGuiSprite.CachedTexture;
			}
		}

		private Rect InflateRect(Rect rect)
		{
			rect.xMin -= inflate;
			rect.yMin -= inflate;
			rect.xMax += inflate;
			rect.yMax += inflate;

			return rect;
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			LeanGuiSprite.UpdateCache();

			vh.Clear();

			var rect   = InflateRect(rectTransform.rect);
			var limit  = Mathf.Min(rect.width / 2.0f, rect.height / 2.0f);
			var center = rect.center;
			var radTL  = Mathf.Max(0.0f, radiusTL >= 0.0f ? radiusTL : radius);
			var radTR  = Mathf.Max(0.0f, radiusTR >= 0.0f ? radiusTR : radius);
			var radBR  = Mathf.Max(0.0f, radiusBR >= 0.0f ? radiusBR : radius);
			var radBL  = Mathf.Max(0.0f, radiusBL >= 0.0f ? radiusBL : radius);

			var poiTL = CalculatePoints(radTL, detailTL);
			var poiTR = CalculatePoints(radTR, detailTR);
			var poiBR = CalculatePoints(radBR, detailBR);
			var poiBL = CalculatePoints(radBL, detailBL);

			Shift(rect.width, ref radTL, ref radTR);
			Shift(rect.width, ref radBL, ref radBR);
			Shift(rect.height, ref radBL, ref radTL);
			Shift(rect.height, ref radBR, ref radTR);

			vert.color = color;
			vert.uv0   = LeanGuiSprite.CachedSolid;

			pointCount = 0;

			SetCorner(0, radTL, poiTL, Mathf.PI * 1.5f);
			SetCorner(1, radTR, poiTR, Mathf.PI * 0.0f);
			SetCorner(2, radBR, poiBR, Mathf.PI * 0.5f);
			SetCorner(3, radBL, poiBL, Mathf.PI * 1.0f);

			corners[0].Limits = new Vector2(rect.width * 0.5f - radTL - radTR, rect.height * 0.5f - radTL - radBL);
			corners[1].Limits = new Vector2(rect.width * 0.5f - radTR - radTL, rect.height * 0.5f - radTR - radBR);
			corners[2].Limits = new Vector2(rect.width * 0.5f - radBR - radBL, rect.height * 0.5f - radBR - radTR);
			corners[3].Limits = new Vector2(rect.width * 0.5f - radBL - radBR, rect.height * 0.5f - radBL - radTL);

			for (var i = 0; i < 4; i++)
			{
				var a = corners[0].Limits;
				a.x = Mathf.Max(a.x, 0.0f);
				a.y = Mathf.Max(a.y, 0.0f);
				corners[0].Limits = a;
			}

			if (thickness < 0.0f)
			{
				if (blur > 0.0f)
				{
					Overflow(blur, limit);

					WriteAll(vh, rect, -blur, true);

					vert.uv0 = LeanGuiSprite.CachedClear;

					WriteAll(vh, rect, blur, false);

					WriteTriangleDisc(vh);

					WriteTriangleRing(vh, 0, pointCount);
				}
				else
				{
					WriteAll(vh, rect, 0.0f, false);

					WriteTriangleDisc(vh);
				}
			}
			else if (thickness > 0.0f)
			{
				if (blur > 0.0f)
				{
					var blur2 = Mathf.Min(blur, thickness * 0.5f);

					WriteAll(vh, rect, blur2 - thickness, true); // Inner
					WriteAll(vh, rect, -blur2, false); // Outer

					vert.uv0 = LeanGuiSprite.CachedClear;

					WriteAll(vh, rect, blur, false); // Outer Blur Edge
					WriteAll(vh, rect, -blur - thickness, true); // Inner Blur Edge

					WriteTriangleRing(vh, 0, pointCount);
					WriteTriangleRing(vh, pointCount, pointCount * 2);
					WriteTriangleRing(vh, pointCount * 3, 0);
				}
				else
				{
					WriteAll(vh, rect, -thickness, true);
					WriteAll(vh, rect, 0.0f, false);

					WriteTriangleRing(vh, 0, pointCount);
				}
			}
		}

		private void WriteAll(VertexHelper vh, Rect rect, float offset, bool limit)
		{
			corners[0].Write(vh, rect.xMin, rect.yMax, offset, limit);
			corners[1].Write(vh, rect.xMax, rect.yMax, offset, limit);
			corners[2].Write(vh, rect.xMax, rect.yMin, offset, limit);
			corners[3].Write(vh, rect.xMin, rect.yMin, offset, limit);
		}

		private void Overflow(float a, float b)
		{
			if (a > b)
			{
				var o = (a - b) / (b * (float)System.Math.E);

				vert.uv0.x += LeanGuiSprite.CachedWidth * o;
			}
		}

		private int CalculatePoints(float radius, float detail)
		{
			return Mathf.Max(1, detail >= 0.0f ? (int)detail : (int)this.detail);
		}

		private void Shift(float size, ref float radiusA, ref float radiusB)
		{
			var over = (radiusA + radiusB) - size;

			if (over > 0.0f)
			{
				if (radiusA > radiusB)
				{
					var max = Mathf.Min(radiusA - radiusB, over);

					over    -= max;
					radiusA -= max;
				}
				else if (radiusA < radiusB)
				{
					var max = Mathf.Min(radiusB - radiusA, over);

					over    -= max;
					radiusB -= max;
				}

				var half = over * 0.5f;

				radiusA -= half;
				radiusB -= half;
			}
		}

		private void SetCorner(int index, float radius, int points, float ao)
		{
			var corner       = corners[index];
			var cornerAngle  = ao + Mathf.PI * 0.25f;
			var cornerNormal = new Vector2(Mathf.Sin(cornerAngle), Mathf.Cos(cornerAngle)) * 1.4142135623730950488016887242097f;

			corner.Radius    = radius;
			corner.DirectionA = cornerNormal;
			corner.Vectors.Clear();

			if (points > 1)
			{
				var step = (Mathf.PI * 0.5f) / (points - 1);

				for (var i = 0; i < points; i++)
				{
					var angle     = i * step + ao;
					var subNormal = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

					corner.Vectors.Add(subNormal);
				}
			}
			else
			{
				corner.Vectors.Add(cornerNormal);
			}

			pointCount += corner.Vectors.Count;
		}

		private void WriteTriangleDisc(VertexHelper vh)
		{
			for (var i = 0; i < pointCount - 2; i++)
			{
				vh.AddTriangle(0, i + 1, i + 2);
			}
		}

		private void WriteTriangleRing(VertexHelper vh, int innerO, int outerO)
		{
			var innerA = innerO;
			var outerA = outerO;

			for (var i = pointCount - 1; i >= 0; i--)
			{
				var innerB = i + innerO;
				var outerB = i + outerO;

				vh.AddTriangle(innerA, innerB, outerA);
				vh.AddTriangle(outerB, outerA, innerB);

				innerA = innerB;
				outerA = outerB;
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
	using UnityEditor;
	using TARGET = LeanBox;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanBox_Editor : CwEditor
	{
		private static bool expandDetail;
		private static bool expandRadius;

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("m_Color", "This allows you to set the color of this element.");
			Draw("m_Material", "This allows you to specify a custom material for this element.");
			Draw("m_RaycastTarget", "Should UI pointers interact with this element?");
			Draw("blur", "This allows you to set the blur radius in local space.");
			Draw("thickness", "This allows you to set the thickness of the border in local space.");

			Draw("inflate", "This allows you to push out or pull in the edges of the box shape in local space.");

			var expandDetail = DrawExpand("detail", "This allows you to set the base detail of the box corners.");

			if (expandDetail == true)
			{
				BeginIndent();
					Draw("detailTL", "This allows you to manually override the detail of the top left corner of this box.\n-1 = No Override", "TL");
					Draw("detailTR", "This allows you to manually override the radius of the top right corner of this box.\n-1 = No Override", "TR");
					Draw("detailBL", "This allows you to manually override the radius of the bottom left corner of this box.\n-1 = No Override", "BL");
					Draw("detailBR", "This allows you to manually override the radius of the bottom right corner of this box.\n-1 = No Override", "BR");
				EndIndent();
			}

			var expandRadius = DrawExpand("radius", "This allows you to set the base radius of the box corners.");

			if (expandRadius == true)
			{
				BeginIndent();
					Draw("radiusTL", "This allows you to manually override the radius of the top left corner of this box.\n-1 = No Override", "TL");
					Draw("radiusTR", "This allows you to manually override the radius of the top right corner of this box.\n-1 = No Override", "TR");
					Draw("radiusBL", "This allows you to manually override the radius of the bottom left corner of this box.\n-1 = No Override", "BL");
					Draw("radiusBR", "This allows you to manually override the radius of the bottom right corner of this box.\n-1 = No Override", "BR");
				EndIndent();
			}
		}

		[MenuItem("GameObject/Lean/GUI/Box", false, 1)]
		private static void CreateBox()
		{
			Selection.activeObject = CwHelper.CreateElement<LeanBox>(Selection.activeTransform);
		}
	}
}
#endif