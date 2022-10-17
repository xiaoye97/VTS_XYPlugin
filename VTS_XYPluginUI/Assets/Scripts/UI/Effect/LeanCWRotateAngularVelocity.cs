using TARGET = CW.Common.CwRotate;

namespace Lean.Transition.Method
{
	[UnityEngine.AddComponentMenu(LeanTransition.MethodsMenuPrefix + "CwRotate.AngularVelocity")]
	public class LeanCWRotateAngularVelocity : LeanMethodWithStateAndTarget
	{
		public override System.Type GetTargetType()
		{
			return typeof(TARGET);
		}

		public override void Register()
		{
			PreviousState = Register(GetAliasedTarget(Data.Target), Data.Value, Data.Duration, Data.Ease);
		}

		public static LeanState Register(TARGET target, UnityEngine.Vector2 value, float duration, LeanEase ease = LeanEase.Smooth)
		{
			var state = LeanTransition.SpawnWithTarget(State.Pool, target);

			state.Value = value;

			state.Ease = ease;

			return LeanTransition.Register(state, duration);
		}

		[System.Serializable]
		public class State : LeanStateWithTarget<TARGET>
		{
			[UnityEngine.Serialization.FormerlySerializedAs("AngularVelocity")] public UnityEngine.Vector3 Value;

			[UnityEngine.Tooltip("This allows you to control how the transition will look.")]
			public LeanEase Ease = LeanEase.Smooth;

			[System.NonSerialized] private UnityEngine.Vector3 oldValue;

			public override int CanFill
			{
				get
				{
					return Target != null && Target.AngularVelocity != Value ? 1 : 0;
				}
			}

			public override void FillWithTarget()
			{
				Value = Target.AngularVelocity;
			}

			public override void BeginWithTarget()
			{
				oldValue = Target.AngularVelocity;
			}

			public override void UpdateWithTarget(float progress)
			{
				Target.AngularVelocity = UnityEngine.Vector3.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
			}

			public static System.Collections.Generic.Stack<State> Pool = new System.Collections.Generic.Stack<State>(); public override void Despawn() { Pool.Push(this); }
		}

		public State Data;
	}
}

namespace Lean.Transition
{
	public static partial class LeanExtensions
	{
		public static TARGET sizeDeltaTransition(this TARGET target, UnityEngine.Vector2 value, float duration, LeanEase ease = LeanEase.Smooth)
		{
			Method.LeanCWRotateAngularVelocity.Register(target, value, duration, ease); return target;
		}
	}
}