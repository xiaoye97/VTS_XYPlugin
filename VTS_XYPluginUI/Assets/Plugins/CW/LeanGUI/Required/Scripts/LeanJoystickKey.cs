using UnityEngine;
using Lean.Common;
using CW.Common;

namespace Lean.Gui
{
	/// <summary>This component moves the sibling joystick in the specified direction while you hold the specified key down.</summary>
	[RequireComponent(typeof(LeanJoystick))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanJoystickKey")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Joystick Key")]
	public class LeanJoystickKey : MonoBehaviour
	{
		/// <summary>The key that you must press for this component to add its delta to the joystick.</summary>
		public KeyCode Key { set { key = value; } get { return key; } } [SerializeField] private KeyCode key;

		/// <summary>The joystick handle will be moved by this many units.
		/// X = Right.
		/// Y = Up.</summary>
		public Vector2 Delta { set { delta = value; } get { return delta; } } [SerializeField] private Vector2 delta = new Vector2(0.0f, 10.0f);

		/// <summary>Multiply the delta by <b>Time.deltaTime</b> before use?</summary>
		public bool ScaleByTime { set { scaleByTime = value; } get { return scaleByTime; } } [SerializeField] private bool scaleByTime;

		[System.NonSerialized]
		private LeanJoystick cachedJoystick;

		protected virtual void OnEnable()
		{
			cachedJoystick = GetComponent<LeanJoystick>();
		}

		protected virtual void Update()
		{
			if (CwInput.GetKeyIsHeld(key) == true)
			{
				var finalDelta = delta;

				if (scaleByTime == true)
				{
					finalDelta *= Time.deltaTime;
				}

				cachedJoystick.IncrementNextValue(finalDelta);
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
	using UnityEditor;
	using TARGET = LeanJoystickKey;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanJoystickKey_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("key", "The key that you must press for this component to add its delta to the joystick.");
			Draw("delta", "The joystick handle will be moved by this many units.\n\nX = Right.\n\nY = Up.");
			Draw("scaleByTime", "Multiply the delta by <b>Time.deltaTime</b> before use?");
		}
	}
}
#endif