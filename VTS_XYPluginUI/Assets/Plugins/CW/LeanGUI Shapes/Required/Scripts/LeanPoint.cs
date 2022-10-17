using UnityEngine;
using Lean.Common;
using CW.Common;

namespace Lean.Gui
{
	/// <summary>This component allows you to draw a UI element on top of a point in the scene (e.g. 3D object).
	/// NOTE: To see the point you also need to add a <b>Graphic</b> to your point GameObject, like <b>LeanBox</b> or <b>Image</b>.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanPoint")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Point")]
	public class LeanPoint : MonoBehaviour
	{
		/// <summary>The camera rendering the target transform/position.
		/// None/Null = The <b>MainCamera</b> will be used.</summary>
		public Camera WorldCamera { set { worldCamera = value; } get { return worldCamera; } } [SerializeField] private Camera worldCamera;

		/// <summary>The <b>RectTransform</b> will be placed above this anchor point.</summary>
		public LeanAnchor Anchor { get { if (anchor == null) anchor = new LeanAnchor(); return anchor; } } [SerializeField] private LeanAnchor anchor;

		[System.NonSerialized]
		private RectTransform cachedRectTransform;

		[System.NonSerialized]
		private bool cachedRectTransformSet;

		/// <summary>This method forces the <b>RectTransform</b> settings to update based on the current line settings.
		/// NOTE: This is automatically called in <b>LateUpdate</b>, but you can manually call it if you have modified it after this, but before the canvas is drawn.</summary>
		[ContextMenu("Update Point")]
		public void UpdatePoint()
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
				var point               = default(Vector3);

				if (Anchor.TryGetPoint(canvasRectTransform, ref point, worldCamera) == true)
				{
					// Position to point
					SetPosition(cachedRectTransform, point);
				}
				else
				{
					// Position outside view
					SetPosition(cachedRectTransform, new Vector3(100000.0f, 100000.0f));
				}
			}
		}

		protected virtual void LateUpdate()
		{
			UpdatePoint();
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
	using TARGET = LeanPoint;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanPoint_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("worldCamera", "The camera rendering the target transform/position.\n\nNone/Null = The MainCamera will be used.");
			Draw("anchor", "The RectTransform will be placed above this anchor point.");
		}

		[MenuItem("GameObject/Lean/GUI/Point", false, 1)]
		private static void CreatePoint()
		{
			Selection.activeObject = CwHelper.CreateElement<LeanPoint>(Selection.activeTransform);
		}
	}
}
#endif