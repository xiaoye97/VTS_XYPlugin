using UnityEngine;
using Lean.Common;
using CW.Common;

namespace Lean.Gui
{
	/// <summary>This component allows you to draw a UI element between two points in the scene (e.g. 3D objects).
	/// NOTE: To see the line you also need to add a <b>Graphic</b> to your line GameObject, like <b>LeanBox</b> or <b>Image</b>.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanLine")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Line")]
	public class LeanLine : MonoBehaviour
	{
		/// <summary>The camera rendering the target transform/position.
		/// None/Null = The <b>MainCamera</b> will be used.</summary>
		public Camera WorldCamera { set { worldCamera = value; } get { return worldCamera; } } [SerializeField] private Camera worldCamera;

		/// <summary>The start point of the <b>RectTransform</b> will be placed at this anchor point.</summary>
		public LeanAnchor AnchorA { get { if (anchorA == null) anchorA = new LeanAnchor(); return anchorA; } } [SerializeField] private LeanAnchor anchorA;

		/// <summary>The end point of the <b>RectTransform</b> will be placed at this anchor point.</summary>
		public LeanAnchor AnchorB { get { if (anchorB == null) anchorB = new LeanAnchor(); return anchorB; } } [SerializeField] private LeanAnchor anchorB;

		[System.NonSerialized]
		private RectTransform cachedRectTransform;

		[System.NonSerialized]
		private bool cachedRectTransformSet;

		/// <summary>This method forces the <b>RectTransform</b> settings to update based on the current line settings.
		/// NOTE: This is automatically called in <b>LateUpdate</b>, but you can manually call it if you have modified it after this, but before the canvas is drawn.</summary>
		[ContextMenu("Update Line")]
		public void UpdateLine()
		{
			var canvas = gameObject.GetComponentInParent<Canvas>();

			if (canvas != null)
			{
				if (cachedRectTransformSet == false)
				{
					cachedRectTransform    = GetComponent<RectTransform>();
					cachedRectTransformSet = true;
				}

				var canvasRectTransform = canvas.transform as RectTransform;
				var pointA              = default(Vector3);
				var pointB              = default(Vector3);
				var pointAValid         = AnchorA.TryGetPoint(canvasRectTransform, ref pointA, worldCamera);
				var pointBValid         = AnchorB.TryGetPoint(canvasRectTransform, ref pointB, worldCamera);

				if (pointAValid == true || pointBValid == true)
				{
					var pointV      = pointB - pointA;
					var pointM      = (pointA + pointB) * 0.5f;
					var sizeDelta   = cachedRectTransform.sizeDelta;
					var scaleFactor = canvas.scaleFactor;

					// Calculate length (depends on render mode)
					if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
					{
						SetPosition(cachedRectTransform, pointA);
						var sampleA = cachedRectTransform.anchoredPosition;
						SetPosition(cachedRectTransform, pointB);
						var sampleB = cachedRectTransform.anchoredPosition;

						sizeDelta.y = Vector2.Distance(sampleA, sampleB);
					}
					else
					{
						sizeDelta.y = pointV.magnitude;

						if (scaleFactor != 0.0f)
						{
							sizeDelta.y /= scaleFactor;
						}
					}

					cachedRectTransform.sizeDelta = sizeDelta;

					// Position in middle
					SetPosition(cachedRectTransform, pointM);

					// Rotate to pointV
					var angle = Mathf.Atan2(pointV.x, pointV.y) * Mathf.Rad2Deg;

					cachedRectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, -angle);
				}
				else
				{
					// Position outside view
					SetPosition(cachedRectTransform, new Vector3(100000.0f, 100000.0f));

					cachedRectTransform.sizeDelta = Vector2.zero;
				}
			}
		}

		protected virtual void LateUpdate()
		{
			UpdateLine();
		}

		private static void SetPosition(RectTransform rectTransform, Vector3 position)
		{
			rectTransform.position = position;

			// NOTE: This is required to force the RectTransform position to update on some versions of the UI?!
			rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
	using UnityEditor;
	using TARGET = LeanLine;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanLine_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("worldCamera", "The camera rendering the target transform/position.\n\nNone/Null = The MainCamera will be used.");

			Separator();

			Draw("anchorA", "The start point of the RectTransform will be placed at this anchor point.");
			Draw("anchorB", "The end point of the RectTransform will be placed at this anchor point.");
		}

		[MenuItem("GameObject/Lean/GUI/Line", false, 1)]
		private static void CreateLine()
		{
			Selection.activeObject = CwHelper.CreateElement<LeanLine>(Selection.activeTransform);
		}
	}
}
#endif