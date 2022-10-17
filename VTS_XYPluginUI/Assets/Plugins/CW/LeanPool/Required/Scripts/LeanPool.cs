using UnityEngine;
using System.Collections.Generic;
using Lean.Common;

namespace Lean.Pool
{
	/// <summary>This class handles the association between a spawned prefab, and the LeanGameObjectPool component that manages it.</summary>
	public static class LeanPool
	{
		public const string HelpUrlPrefix = LeanCommon.HelpUrlPrefix + "LeanPool#";

		public const string ComponentPathPrefix = "Lean/Pool/Lean ";

		/// <summary>When you spawn a prefab from this class, the association between the spawned clone and the pool that spawned it will be stored here so it can quickly be despawned.</summary>
		public static Dictionary<GameObject, LeanGameObjectPool> Links = new Dictionary<GameObject, LeanGameObjectPool>();

		/// <summary>This allows you to spawn a prefab via Component.</summary>
		public static T Spawn<T>(T prefab, Transform parent, bool worldPositionStays = false)
			where T : Component
		{
			if (prefab == null) { Debug.LogError("Attempting to spawn a null prefab."); return null; }
			var clone = Spawn(prefab.gameObject, parent, worldPositionStays); return clone != null ? clone.GetComponent<T>() : null;
		}

		/// <summary>This allows you to spawn a prefab via Component.</summary>
		public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
			where T : Component
		{
			if (prefab == null) { Debug.LogError("Attempting to spawn a null prefab."); return null; }
			var clone = Spawn(prefab.gameObject, position, rotation, parent); return clone != null ? clone.GetComponent<T>() : null;
		}

		/// <summary>This allows you to spawn a prefab via Component.</summary>
		public static T Spawn<T>(T prefab)
			where T : Component
		{
			if (prefab == null) { Debug.LogError("Attempting to spawn a null prefab."); return null; }
			var clone = Spawn(prefab.gameObject); return clone != null ? clone.GetComponent<T>() : null;
		}

		/// <summary>This allows you to spawn a prefab via GameObject.</summary>
		public static GameObject Spawn(GameObject prefab, Transform parent, bool worldPositionStays = false)
		{
			if (prefab == null) { Debug.LogError("Attempting to spawn a null prefab."); return null; }
			var transform = prefab.transform;
			if (parent != null && worldPositionStays == true)
			{
				return Spawn(prefab, prefab.transform.position, Quaternion.identity, Vector3.one, parent, worldPositionStays);
			}
			return Spawn(prefab, transform.localPosition, transform.localRotation, transform.localScale, parent, false);
		}

		/// <summary>This allows you to spawn a prefab via GameObject.</summary>
		public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (prefab == null) { Debug.LogError("Attempting to spawn a null prefab."); return null; }
			if (parent != null)
			{
				position = parent.InverseTransformPoint(position);
				rotation = Quaternion.Inverse(parent.rotation) * rotation;
			}
			return Spawn(prefab, position, rotation, prefab.transform.localScale, parent, false);
		}

		/// <summary>This allows you to spawn a prefab via GameObject.</summary>
		public static GameObject Spawn(GameObject prefab)
		{
			if (prefab == null) { Debug.LogError("Attempting to spawn a null prefab."); return null; }
			var transform = prefab.transform;
			return Spawn(prefab, transform.localPosition, transform.localRotation, transform.localScale, null, false);
		}

		/// <summary>This allows you to spawn a prefab via GameObject.</summary>
		private static GameObject Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
			if (prefab != null)
			{
				// Find the pool that handles this prefab
				var pool = default(LeanGameObjectPool);

				// Create a new pool for this prefab?
				if (LeanGameObjectPool.TryFindPoolByPrefab(prefab, ref pool) == false)
				{
					pool = new GameObject("LeanPool (" + prefab.name + ")").AddComponent<LeanGameObjectPool>();

					pool.Prefab = prefab;
				}

				// Try and spawn a clone from this pool
				var clone = default(GameObject);

				if (pool.TrySpawn(ref clone, localPosition, localRotation, localScale, parent, worldPositionStays) == true)
				{
					// Clone already registered?
					if (Links.Remove(clone) == true)
					{
						// If this pool recycles clones, then this can be expected
						if (pool.Recycle == true)
						{
							
						}
						// This shouldn't happen
						else
						{
							Debug.LogWarning("You're attempting to spawn a clone that hasn't been despawned. Make sure all your Spawn and Despawn calls match, you shouldn't be manually destroying them!", clone);
						}
					}

					// Associate this clone with this pool
					Links.Add(clone, pool);

					return clone;
				}
			}
			else
			{
				Debug.LogError("Attempting to spawn a null prefab.");
			}

			return null;
		}

		/// <summary>This will call <b>DespawnAll</b> on all active and enabled pools.</summary>
		public static void DespawnAll()
		{
			foreach (var instance in LeanGameObjectPool.Instances)
			{
				instance.DespawnAll();
			}

			Links.Clear();
		}

		/// <summary>This allows you to despawn a clone via Component, with optional delay.</summary>
		public static void Despawn(Component clone, float delay = 0.0f)
		{
			if (clone != null) Despawn(clone.gameObject, delay);
		}

		/// <summary>This allows you to despawn a clone via GameObject, with optional delay.</summary>
		public static void Despawn(GameObject clone, float delay = 0.0f)
		{
			if (clone != null)
			{
				var pool = default(LeanGameObjectPool);

				// Try and find the pool associated with this clone
				if (Links.TryGetValue(clone, out pool) == true)
				{
					// Remove the association
					Links.Remove(clone);

					pool.Despawn(clone, delay);
				}
				else
				{
					if (LeanGameObjectPool.TryFindPoolByClone(clone, ref pool) == true)
					{
						pool.Despawn(clone, delay);
					}
					else
					{
						Debug.LogWarning("You're attempting to despawn a gameObject that wasn't spawned from a pool (or the pool was destroyed).", clone);

						// Fall back to normal destroying
#if UNITY_EDITOR
						if (Application.isPlaying == false)
						{
							Object.DestroyImmediate(clone);

							return;
						}
#endif
						if (delay > 0.0f)
						{
							Object.Destroy(clone);
						}
						else
						{
							Object.Destroy(clone, delay);
						}
					}
				}
			}
			else
			{
				Debug.LogWarning("You're attempting to despawn a null gameObject.", clone);
			}
		}

		/// <summary>This allows you to detach a clone via Component.
		/// A detached clone will act as a normal GameObject, requiring you to manually destroy or otherwise manage it.
		/// NOTE: If this clone has been despawned then it will still be parented to the pool.</summary>
		public static void Detach(Component clone, bool detachFromPool = true)
		{
			if (clone != null) Detach(clone.gameObject, detachFromPool);
		}

		/// <summary>This allows you to detach a clone via GameObject.
		/// A detached clone will act as a normal GameObject, requiring you to manually destroy or otherwise manage it.
		/// NOTE: If this clone has been despawned then it will still be parented to the pool.</summary>
		public static void Detach(GameObject clone, bool detachFromPool)
		{
			if (clone != null)
			{
				if (detachFromPool == true)
				{
					var pool = default(LeanGameObjectPool);

					// Try and find the pool associated with this clone
					if (Links.TryGetValue(clone, out pool) == true)
					{
						// Remove the association
						Links.Remove(clone);

						pool.Detach(clone);
					}
					else
					{
						if (LeanGameObjectPool.TryFindPoolByClone(clone, ref pool) == true)
						{
							pool.Detach(clone);
						}
						else
						{
							Debug.LogWarning("You're attempting to detach a GameObject that wasn't spawned from any pool (or its pool was destroyed).", clone);
						}
					}
				}
				else
				{
					Links.Remove(clone);
				}
			}
			else
			{
				Debug.LogWarning("You're attempting to detach a null GameObject.", clone);
			}
		}
	}
}