using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Lean.Common;
using CW.Common;

namespace Lean.Gui
{
	/// <summary>This component allows you to create UI elements with a custom polygon shape.</summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasRenderer))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanPolygon")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Polygon")]
	public class LeanPolygon : MaskableGraphic
	{
		/// <summary>This allows you to set the blur radius in local space.</summary>
		public float Blur { set { blur = value; } get { return blur; } } [SerializeField] private float blur = 0.5f;

		/// <summary>This allows you to set the thickness of the border in local space.</summary>
		public float Thickness { set { thickness = value; } get { return thickness; } } [SerializeField] private float thickness = -1.0f;

		/// <summary>This list stores all polygon points in local space.
		/// NOTE: If you modify this from code, then you must manually call the <b>SetVerticesDirty</b> method on this component.</summary>
		public List<Vector4> Points { get { if (points == null) points = new List<Vector4>(); return points; } } [SerializeField] private List<Vector4> points;

		private static Texture2D blurTexture;

		private static bool blurTextureSet;

		private static UIVertex vert = UIVertex.simpleVert;

		private static List<Vector2> positions = new List<Vector2>();

		private static List<Vector2> shiftedPositions = new List<Vector2>();

		private static List<Vector2> normals = new List<Vector2>();

		public override Texture mainTexture
		{
			get
			{
				return LeanGuiSprite.CachedTexture;
			}
		}

		/// <summary>This method allows you to reset the .Z and .W anchor values of all <b>Point</b> values to 0.5, 0.5.</summary>
		[ContextMenu("Reset Points.ZW")]
		public void ResetPointsZW()
		{
			if (points != null)
			{
				for (var i = points.Count - 1; i >= 0; i--)
				{
					var point = points[i];

					point.z = 0.5f;
					point.w = 0.5f;

					points[i] = point;
					
					SetVerticesDirty();
#if UNITY_EDITOR
					UnityEditor.EditorUtility.SetDirty(this);
#endif
				}
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			LeanGuiSprite.UpdateCache();

			vh.Clear();

			var rect = rectTransform.rect;

			positions.Clear();

			if (points != null)
			{
				for (var i = 0; i < points.Count; i++)
				{
					var p = points[i];
					var x = rect.xMin + rect.width  * p.z + p.x;
					var y = rect.yMin + rect.height * p.w + p.y;

					positions.Add(new Vector2(x, y));
				}
			}

			if (CalculateNormals() == true)
			{
				vert.color = color;
				vert.uv0   = LeanGuiSprite.CachedSolid;

				if (thickness < 0.0f)
				{
					if (blur > 0.0f)
					{
						WriteVertexRing(vh, -blur); WriteTriangleDisc(vh);

						vert.uv0 = LeanGuiSprite.CachedClear;

						WriteVertexRing(vh, blur); WriteTriangleRing(vh, 0, LeanTriangulation.Points.Count);
					}
					else
					{
						WriteVertexRing(vh, 0.0f); WriteTriangleDisc(vh);
					}
				}
				else if (thickness > 0.0f)
				{
					if (blur > 0.0f)
					{
						WriteVertexRing(vh, blur - thickness); // Inner
						WriteVertexRing(vh, -blur); // Outer

						vert.uv0 = LeanGuiSprite.CachedClear;

						WriteVertexRing(vh, blur); // Outer Blur Edge
						WriteVertexRing(vh, -blur - thickness); // Inner Blur Edge

						WriteTriangleRing(vh, 0, positions.Count);
						WriteTriangleRing(vh, positions.Count, positions.Count * 2);
						WriteTriangleRing(vh, positions.Count * 3, 0);
					}
					else
					{
						WriteVertexRing(vh, -thickness);
						WriteVertexRing(vh, 0.0f);

						WriteTriangleRing(vh, 0, positions.Count);
					}
				}
			}
		}

		private void WriteTriangleDisc(VertexHelper vh)
		{
			if (LeanTriangulation.Calculate(shiftedPositions) == true)
			{
				for (var i = 0; i < LeanTriangulation.Triangles.Count; i++)
				{
					var triangle = LeanTriangulation.Triangles[i]; vh.AddTriangle(triangle.IndexA, triangle.IndexB, triangle.IndexC);
				}
			}
		}

		private void WriteTriangleRing(VertexHelper vh, int innerO, int outerO)
		{
			var innerA = innerO;
			var outerA = outerO;

			for (var i = positions.Count - 1; i >= 0; i--)
			{
				var innerB = i + innerO;
				var outerB = i + outerO;

				vh.AddTriangle(innerA, innerB, outerA);
				vh.AddTriangle(outerB, outerA, innerB);

				innerA = innerB;
				outerA = outerB;
			}
		}

		private void WriteVertexRing(VertexHelper vh, float distance)
		{
			shiftedPositions.Clear();

			for (var i = 0; i < positions.Count; i++)
			{
				var position = positions[i] + distance * normals[i];

				shiftedPositions.Add(position);

				vert.position = position;

				vh.AddVert(vert);
			}
		}

		private bool CalculateNormals()
		{
			if (positions != null && positions.Count > 2)
			{
				normals.Clear();

				var count   = positions.Count;
				var normalA = CalculateNormal(positions[count - 1] - positions[0]);

				for (var i = 0; i < positions.Count; i++)
				{
					var normalB = CalculateNormal(positions[i] - positions[(i + 1) % count]);
					var inset   = normalA + normalB;
					var mag     = inset.sqrMagnitude;
					var direction = Vector2.zero;

					if (mag > 0.0f)
					{
						mag = Mathf.Sqrt(mag);

						direction = (inset / mag) / mag;
					}

					normals.Add(-direction);

					normalA = normalB;
				}

				return true;
			}

			return false;
		}

		private static Vector2 CalculateNormal(Vector2 vector)
		{
			return new Vector2(-vector.y, vector.x).normalized;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
	using UnityEditor;
	using TARGET = LeanPolygon;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanPolygon_Editor : CwEditor
	{
		enum PresetType
		{
			Box,
			Circle,
			AnchoredBox
		}

		private bool editPoints;

		private Tool editTool;

		private int draggingPoint = -1;

		private static GUIStyle downStyle;

		private static Rect cachedRect;

		private TARGET tgt;

		public GUIStyle EditStyle
		{
			get
			{
				if (editPoints == true)
				{
					if (downStyle == null)
					{
						downStyle = new GUIStyle(EditorStyles.miniButton);

						downStyle.normal.background = downStyle.onActive.background;
					}

					return downStyle;
				}

				return EditorStyles.miniButton;
			}
		}

		private PresetType preset;
		private int        presetPoints = 10;
		private float      presetRadius = 50.0f;
		private Vector2    presetCenter;
		private Vector2    presetSize = new Vector2(50.0f, 50.0f);

		protected override void OnInspector()
		{
			TARGET[] tgts; GetTargets(out tgt, out tgts);

			cachedRect = tgt.rectTransform.rect;

			DrawEdit();
			Draw("m_Color", "This allows you to set the color of this element.");
			Draw("m_Material", "This allows you to specify a custom material for this element.");
			Draw("m_RaycastTarget", "Should UI pointers interact with this element?");
			Draw("blur", "This allows you to set the blur radius in local space.");
			Draw("thickness", "This allows you to set the thickness of the border in local space.");
			Draw("points", "This list stores all polygon points in local space.\n\nNOTE: If you modify this from code, then you must manually call the <b>SetVerticesDirty</b> method on this component.");

			if (tgt.Points.Count == 0)
			{
				Separator();

				EditorGUILayout.HelpBox("This polygon has no points, so it will be invisible. You can manually add points, or use a preset below. Keep in mind the points must be in clockwise order.", MessageType.Info);

				preset = (PresetType)EditorGUILayout.EnumPopup("Preset:", preset);

				BeginIndent();
					switch (preset)
					{
						case PresetType.Box:
						{
							presetSize   = EditorGUILayout.Vector2Field("Size", presetSize);
							presetCenter = EditorGUILayout.Vector2Field("Center", presetCenter);

							if (GUILayout.Button("Create") == true)
							{
								Undo.RecordObject(tgt, "Create Preset: Box");

								var half = presetSize * 0.5f;

								tgt.Points.Add(new Vector4(-half.x + presetCenter.x, -half.y + presetCenter.y, 0.0f, 0.0f));
								tgt.Points.Add(new Vector4(-half.x + presetCenter.x,  half.y + presetCenter.y, 0.0f, 0.0f));
								tgt.Points.Add(new Vector4( half.x + presetCenter.x,  half.y + presetCenter.y, 0.0f, 0.0f));
								tgt.Points.Add(new Vector4( half.x + presetCenter.x, -half.y + presetCenter.y, 0.0f, 0.0f));

								NotifyModified();
							}
						}
						break;

						case PresetType.Circle:
						{
							presetPoints = EditorGUILayout.IntField("Point Count", presetPoints);
							presetRadius = EditorGUILayout.FloatField("Radius", presetRadius);
							presetCenter = EditorGUILayout.Vector2Field("Center", presetCenter);

							if (GUILayout.Button("Create") == true)
							{
								Undo.RecordObject(tgt, "Create Preset: Circle");

								var step = Mathf.PI * 2.0f / presetPoints;

								for (var i = 0; i < presetPoints; i++)
								{
									var x = Mathf.Sin(i * step) * presetRadius + presetCenter.x;
									var y = Mathf.Cos(i * step) * presetRadius + presetCenter.y;

									tgt.Points.Add(new Vector4(x, y, 0.0f, 0.0f));
								}

								NotifyModified();
							}
						}
						break;

						case PresetType.AnchoredBox:
						{
							if (GUILayout.Button("Create") == true)
							{
								Undo.RecordObject(tgt, "Create Preset: Anchored Box");

								tgt.Points.Add(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
								tgt.Points.Add(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
								tgt.Points.Add(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
								tgt.Points.Add(new Vector4(0.0f, 0.0f, 1.0f, 0.0f));

								NotifyModified();
							}
						}
						break;
					}
				EndIndent();
			}
		}

		protected virtual void OnDisable()
		{
			EndEdit();
		}

		private Vector2 GetPoint(int i)
		{
			var p = tgt.Points[i];
			var x = cachedRect.xMin + cachedRect.width  * p.z + p.x;
			var y = cachedRect.yMin + cachedRect.height * p.w + p.y;

			return new Vector2(x, y);
		}

		private void SetPoint(int i, Vector2 n)
		{
			var p = tgt.Points[i];
			var x = cachedRect.xMin + cachedRect.width  * p.z;
			var y = cachedRect.yMin + cachedRect.height * p.w;

			p.x = n.x - x;
			p.y = n.y - y;

			tgt.Points[i] = p;
		}

		private void BeginEdit()
		{
			editPoints = true;

			editTool = Tools.current;

			Tools.current = Tool.None;
		}

		private void EndEdit()
		{
			editPoints = false;

			Tools.current = editTool;
		}

		private void DrawEdit()
		{
			var rect = Reserve(); rect.xMin += EditorGUIUtility.labelWidth;

			if (GUI.Button(rect, "Edit Points", EditStyle) == true)
			{
				if (editPoints == true)
				{
					EndEdit();
				}
				else
				{
					BeginEdit();
				}
			}
		}

		private Vector2 GetMouseLocalPoint()
		{
			var ray  = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			var dist = default(float);

			if (new Plane(tgt.transform.forward, tgt.transform.position).Raycast(ray, out dist) == true)
			{
				return tgt.transform.InverseTransformPoint(ray.GetPoint(dist));
			}

			return default(Vector2);
		}

		private int GetClosestPoint(ref float foundDistance)
		{
			var localPoint   = GetMouseLocalPoint();
			var bestIndex    = -1;
			var bestDistance = float.PositiveInfinity;

			for (var i = 0; i < tgt.Points.Count; i++)
			{
				var distance = Vector2.Distance(GetPoint(i), localPoint);

				if (distance < bestDistance)
				{
					bestDistance = distance;
					bestIndex    = i;
				}
			}

			foundDistance = bestDistance;

			return bestIndex;
		}

		private void DrawPoint(Vector2 localPoint)
		{
			var screenPoint = Camera.current.WorldToScreenPoint(tgt.transform.TransformPoint(localPoint));
			var rect        = new Rect(0.0f, 0.0f, 7.0f, 7.0f); rect.center = new Vector2(screenPoint.x, Screen.height - screenPoint.y - 37.0f);

			GUI.DrawTexture(rect, Texture2D.whiteTexture);
		}

		private static float GetDistance(Vector2 a, Vector2 b, Vector2 p, ref Vector2 closest)
		{
			var ba   = b - a;
			var baba = Vector2.Dot(ba, ba);

			if (baba != 0.0f)
			{
				var d = Vector2.Dot(p - a, ba) / baba;

				if (d >= 0.0f && d <= 1.0f)
				{
					closest = a + ba * d;

					return Vector2.Distance(closest, p);
				}
			}

			return float.PositiveInfinity;
		}

		private int GetClosestEdge(ref float bestDistance, ref Vector2 bestPoint)
		{
			var localPoint = GetMouseLocalPoint();
			var bestIndex  = -1;
			
			bestDistance = float.PositiveInfinity;

			for (var i = 0; i < tgt.Points.Count; i++)
			{
				var point    = default(Vector2);
				var distance = GetDistance(GetPoint(i), GetPoint((i + 1) % tgt.Points.Count), localPoint, ref point);

				if (distance < bestDistance)
				{
					bestDistance = distance;
					bestPoint    = point;
					bestIndex    = i;
				}
			}

			return bestIndex;
		}

		private void NotifyModified()
		{
			EditorUtility.SetDirty(tgt);

			tgt.SetVerticesDirty();
		}

		private float GetViewScale(float pixels)
		{
			var c = SceneView.currentDrawingSceneView.camera;
			var d = Vector3.Distance(c.transform.position, tgt.transform.position);
			var a = c.ScreenToWorldPoint(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f - pixels, d));
			var b = c.ScreenToWorldPoint(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f + pixels, d));
			
			a = tgt.transform.InverseTransformPoint(a);
			b = tgt.transform.InverseTransformPoint(b);

			return Vector2.Distance(a, b);
		}

		public virtual void OnSceneGUI()
		{
			tgt = (TARGET)target;

			if (Event.current.type == EventType.MouseUp)
			{
				draggingPoint = -1;
			}

			Handles.matrix = tgt.transform.localToWorldMatrix;
			Handles.color  = new Color(0.5f, 1.0f, 0.5f);

			for (var i = 0; i < tgt.Points.Count; i++)
			{
				var pointA = GetPoint(i);
				var pointB = GetPoint((i + 1) % tgt.Points.Count);

				Handles.DrawLine(pointA, pointB);
			}

			var control = Event.current.control;
			var down    = Event.current.type == EventType.MouseDown && Event.current.button == 0;

			if (editPoints == true)
			{
				if (Event.current.type == EventType.Layout)
				{
					HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
				}

				SceneView.currentDrawingSceneView.Repaint();

				var snapDistance = GetViewScale(5.0f);

				Handles.BeginGUI();
				{
					var closestDist  = default(float);
					var closestIndex = GetClosestPoint(ref closestDist);

					// Remove
					if (control == true)
					{
						GUI.color = new Color(1.0f, 0.0f, 0.0f);

						if (closestDist < snapDistance)
						{
							DrawPoint(GetPoint(closestIndex));

							if (Event.current.type == EventType.MouseDown)
							{
								tgt.Points.RemoveAt(closestIndex); NotifyModified();
							}
						}
					}
					// Move/split
					else
					{
						GUI.color = new Color(0.0f, 1.0f, 0.0f);

						if (draggingPoint >= 0)
						{
							SetPoint(draggingPoint, GetMouseLocalPoint()); NotifyModified();

							DrawPoint(GetPoint(draggingPoint));
						}
						else
						{
							if (closestDist < snapDistance)
							{
								DrawPoint(GetPoint(closestIndex));

								if (down == true)
								{
									draggingPoint = closestIndex;
								}
							}
							else
							{
								var createDist  = default(float);
								var createPoint = default(Vector2);
								var createIndex = GetClosestEdge(ref createDist, ref createPoint);

								if (createDist < 20.0f)
								{
									GUI.color = new Color(1.0f, 1.0f, 0.0f);

									DrawPoint(createPoint);

									if (down == true)
									{
										tgt.Points.Insert(createIndex + 1, createPoint); NotifyModified();

										draggingPoint = createIndex + 1;
									}
								}
							}
						}
					}
				}
				Handles.EndGUI();

				if (GUI.changed == true)
				{
					EditorUtility.SetDirty(target);
				}
			}
		}

		[MenuItem("GameObject/Lean/GUI/Polygon", false, 1)]
		public static void CreatePolygon()
		{
			Selection.activeObject = CwHelper.CreateElement<LeanPolygon>(Selection.activeTransform);
		}
	}
}
#endif