using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This class allows you to define an anchor point in the scene based on a <b>Transform</b> and offsets, or a 3D point.
	/// This is used by components like <b>LeanPoint</b> and <b>LeanLine</b>.</summary>
	[System.Serializable]
	public class LeanAnchor
	{
		/// <summary>If you want this anchor to follow a <b>Transform</b>, then set it here.</summary>
		public Transform Transform { set { transform = value; } get { return transform; } } [SerializeField] private Transform transform;

		/// <summary>This allows you to set the local offset of the anchor relative to the <b>Transform</b>.
		/// If no <b>Transform</b> is set, then this will be the world space position.</summary>
		public Vector3 LocalOffset { set { localOffset = value; } get { return localOffset; } } [SerializeField] private Vector3 localOffset;

		/// <summary>This allows you to set the pixel offset of the anchor on the screen.</summary>
		public Vector2 PixelOffset { set { pixelOffset = value; } get { return pixelOffset; } } [SerializeField] private Vector2 pixelOffset;

		public Vector3 GetPoint(RectTransform canvasRectTransform, Camera camera = null)
		{
			var point = default(Vector3);

			TryGetPoint(canvasRectTransform, ref point, camera);

			return point;
		}

		public bool TryGetPoint(RectTransform canvasRectTransform, ref Vector3 point, Camera camera = null)
		{
			if (camera == null)
			{
				camera = Camera.main;
			}

			if (camera != null)
			{
				var worldPoint    = transform != null ? transform.TransformPoint(localOffset) : localOffset;
				var viewportPoint = GetViewportPoint(camera, worldPoint);

				// Convert viewport point to canvas point
				var canvasRect = canvasRectTransform.rect;
				var canvasX    = canvasRect.xMin + canvasRect.width  * viewportPoint.x + pixelOffset.x;
				var canvasY    = canvasRect.yMin + canvasRect.height * viewportPoint.y + pixelOffset.y;

				// Convert canvas point to world point
				point = canvasRectTransform.TransformPoint(canvasX, canvasY, 0.0f);

				// If outside frustum, return false, but keep the point
				if (LeanGui.InvaidViewportPoint(camera, viewportPoint) == true)
				{
					return false;
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		private bool IsWorldSpace
		{
			get
			{
				if (transform != null)
				{
					var canvas = transform.GetComponentInParent<Canvas>();

					if (canvas != null)
					{
						switch (canvas.renderMode)
						{
							case RenderMode.ScreenSpaceOverlay: return false;
							case RenderMode.ScreenSpaceCamera : return true;
						}
					}
				}

				return true;
			}
		}

		private Vector3 GetViewportPoint(Camera camera, Vector3 point)
		{
			if (IsWorldSpace == true)
			{
				return camera.WorldToViewportPoint(point);
			}
			else
			{
				point = RectTransformUtility.WorldToScreenPoint(null, point);
				point.z = 0.5f;

				return camera.ScreenToViewportPoint(point);
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CustomPropertyDrawer(typeof(LeanAnchor))]
	public class LeanAnchorDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var height = base.GetPropertyHeight(property, label);

			return height * 3 + 2.0f * 2;
		}

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			var height = base.GetPropertyHeight(property, label);

			rect.height = height;

			var color = GUI.color; if (property.FindPropertyRelative("transform").objectReferenceValue == null) GUI.color = Color.red;
				DrawProperty(ref rect, property, "transform", label, null, "If you want this anchor to follow a Transform, then set it here.");
			GUI.color = color;

			EditorGUI.indentLevel++;
				DrawProperty(ref rect, property, "localOffset", label, "Local Offset", "This allows you to set the local offset of the anchor relative to the Transform. If no Transform is set, then this will be the world space position.");
				DrawProperty(ref rect, property, "pixelOffset", label, "Pixel Offset", "This allows you to set the pixel offset of the anchor on the screen.");
			EditorGUI.indentLevel--;
		}

		private void DrawProperty(ref Rect rect, SerializedProperty property, string childName, GUIContent label = null, string overrideName = null, string overrideTooltip = null)
		{
			var childProperty = property.FindPropertyRelative(childName);

			if (label != null)
			{
				if (string.IsNullOrEmpty(overrideName) == false)
				{
					label.text    = overrideName;
					label.tooltip = overrideTooltip;
				}

				EditorGUI.PropertyField(rect, childProperty, label);
			}
			else
			{
				EditorGUI.PropertyField(rect, childProperty);
			}

			rect.y += rect.height + 2.0f;
		}
	}
}
#endif