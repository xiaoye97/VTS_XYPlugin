using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using CW.Common;

namespace Lean.Pool
{
	/// <summary>This component allows you to pool GameObjects, giving you a very fast alternative to Instantiate and Destroy.
	/// Pools also have settings to preload, recycle, and set the spawn capacity, giving you lots of control over your spawning.</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanPool.HelpUrlPrefix + "LeanGameObjectPool")]
	[AddComponentMenu(LeanPool.ComponentPathPrefix + "GameObject Pool")]
	public class LeanGameObjectPool : MonoBehaviour, ISerializationCallbackReceiver
	{
		[System.Serializable]
		public class Delay
		{
			public GameObject Clone;
			public float      Life;
		}

		public enum NotificationType
		{
			None,
			SendMessage,
			BroadcastMessage,
			IPoolable,
			BroadcastIPoolable
		}

		public enum StrategyType
		{
			ActivateAndDeactivate,
			DeactivateViaHierarchy
		}

		/// <summary>All active and enabled pools in the scene.</summary>
		public static LinkedList<LeanGameObjectPool> Instances = new LinkedList<LeanGameObjectPool>(); private LinkedListNode<LeanGameObjectPool> instancesNode;

		/// <summary>The prefab this pool controls.</summary>
		public GameObject Prefab { set { if (value != prefab) { UnregisterPrefab(); prefab = value; RegisterPrefab(); } } get { return prefab; } } [SerializeField] private GameObject prefab;

		/// <summary>If you need to perform a special action when a prefab is spawned or despawned, then this allows you to control how that action is performed.
		/// <tip>None</tip>If you use this then you must rely on the OnEnable and OnDisable messages.
		/// <tip>SendMessage</tip>The prefab clone is sent the OnSpawn and OnDespawn messages.
		/// <tip>BroadcastMessage</tip>The prefab clone and all its children are sent the OnSpawn and OnDespawn messages.
		/// <tip>IPoolable</tip>The prefab clone's components implementing IPoolable are called.
		/// <tip>Broadcast IPoolable</tip>The prefab clone and all its child components implementing IPoolable are called.</summary>
		public NotificationType Notification { set { notification = value; } get { return notification; } } [SerializeField] private NotificationType notification = NotificationType.IPoolable;

		/// <summary>This allows you to control how spawned/despawned GameObjects will be handled. The <b>DeactivateViaHierarchy</b> mode should be used if you need to maintain your prefab's de/activation state.
		/// ActivateAndDeactivate = Despawned clones will be deactivated and placed under this GameObject.
		/// DeactivateViaHierarchy = Despawned clones will be placed under a deactivated GameObject and left alone.</summary>
		public StrategyType Strategy { set { strategy = value; } get { return strategy; } } [SerializeField] private StrategyType strategy = StrategyType.ActivateAndDeactivate;

		/// <summary>Should this pool preload some clones?</summary>
		public int Preload { set { preload = value; } get { return preload; } } [SerializeField] private int preload;

		/// <summary>Should this pool have a maximum amount of spawnable clones?</summary>
		public int Capacity { set { capacity = value; } get { return capacity; } } [SerializeField] private int capacity;

		/// <summary>If the pool reaches capacity, should new spawns force older ones to despawn?</summary>
		public bool Recycle { set { recycle = value; } get { return recycle; } } [SerializeField] private bool recycle;

		/// <summary>Should this pool be marked as DontDestroyOnLoad?</summary>
		public bool Persist { set { persist = value; } get { return persist; } } [SerializeField] private bool persist;

		/// <summary>Should the spawned clones have their clone index appended to their name?</summary>
		public bool Stamp { set { stamp = value; } get { return stamp; } } [SerializeField] private bool stamp;

		/// <summary>Should detected issues be output to the console?</summary>
		public bool Warnings { set { warnings = value; } get { return warnings; } } [SerializeField] private bool warnings = true;

		/// <summary>This stores all spawned clones in a list. This is used when Recycle is enabled, because knowing the spawn order must be known. This list is also used during serialization.</summary>
		[SerializeField]
		private List<GameObject> spawnedClonesList = new List<GameObject>();

		/// <summary>This stores all spawned clones in a hash set. This is used when Recycle is disabled, because their storage order isn't important. This allows us to quickly find the Clone associated with the specified GameObject.</summary>
		private HashSet<GameObject> spawnedClonesHashSet = new HashSet<GameObject>();

		/// <summary>All the currently despawned prefab instances.</summary>
		[SerializeField]
		private List<GameObject> despawnedClones = new List<GameObject>();

		/// <summary>All the delayed destruction objects.</summary>
		[SerializeField]
		private List<Delay> delays = new List<Delay>();
		
		[SerializeField]
		private Transform deactivatedChild;

		private static Dictionary<GameObject, LeanGameObjectPool> prefabMap = new Dictionary<GameObject, LeanGameObjectPool>();

		private static List<IPoolable> tempPoolables = new List<IPoolable>();

		/// <summary>If you're using the <b>Strategy = DeactivateViaHierarchy</b> mode, then all despawned clones will be placed under this.</summary>
		public Transform DeactivatedChild
		{
			get
			{
				if (deactivatedChild == null)
				{
					var child = new GameObject("Despawned Clones");

					child.SetActive(false);

					deactivatedChild = child.transform;

					deactivatedChild.SetParent(transform, false);
				}

				return deactivatedChild;
			}
		}

#if UNITY_EDITOR
		/// <summary>This will return false if you have preloaded prefabs do not match the <b>Prefab</b>.
		/// NOTE: This is only available in the editor.</summary>
		public bool DespawnedClonesMatch
		{
			get
			{
				for (var i = despawnedClones.Count - 1; i >= 0; i--)
				{
					var clone = despawnedClones[i];

					if (clone != null && UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(clone) != prefab)
					{
						return false;
					}
				}

				return true;
			}
		}
#endif

		/// <summary>Find the pool responsible for handling the specified prefab.</summary>
		public static bool TryFindPoolByPrefab(GameObject prefab, ref LeanGameObjectPool foundPool)
		{
			return prefabMap.TryGetValue(prefab, out foundPool);
		}

		/// <summary>Find the pool responsible for handling the specified prefab clone.
		/// NOTE: This can be an expensive operation if you have many large pools.</summary>
		public static bool TryFindPoolByClone(GameObject clone, ref LeanGameObjectPool pool)
		{
			foreach (var instance in Instances)
			{
				// Search hash set
				if (instance.spawnedClonesHashSet.Contains(clone) == true)
				{
					pool = instance;

					return true;
				}

				// Search list
				for (var j = instance.spawnedClonesList.Count - 1; j >= 0; j--)
				{
					if (instance.spawnedClonesList[j] == clone)
					{
						pool = instance;

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>Returns the amount of spawned clones.</summary>
		public int Spawned
		{
			get
			{
				return spawnedClonesList.Count + spawnedClonesHashSet.Count;
			}
		}

		/// <summary>Returns the amount of despawned clones.</summary>
		public int Despawned
		{
			get
			{
				return despawnedClones.Count;
			}
		}

		/// <summary>Returns the total amount of spawned and despawned clones.</summary>
		public int Total
		{
			get
			{
				return Spawned + Despawned;
			}
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.
		/// NOTE: This method is designed to work with Unity's event system, so it has no return value.</summary>
		public void Spawn()
		{
			var clone = default(GameObject); TrySpawn(ref clone);
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.
		/// NOTE: This method is designed to work with Unity's event system, so it has no return value.</summary>
		public void Spawn(Vector3 position)
		{
			var clone = default(GameObject); TrySpawn(ref clone, position, transform.localRotation);
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.</summary>
		public GameObject Spawn(Transform parent, bool worldPositionStays = false)
		{
			var clone = default(GameObject); TrySpawn(ref clone, parent, worldPositionStays); return clone;
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.</summary>
		public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			var clone = default(GameObject); TrySpawn(ref clone, position, rotation, parent); return clone;
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.</summary>
		public bool TrySpawn(ref GameObject clone, Transform parent, bool worldPositionStays = false)
		{
			if (prefab == null) { if (warnings == true) Debug.LogWarning("You're attempting to spawn from a pool with a null prefab", this); return false; }
			if (parent != null && worldPositionStays == true)
			{
				return TrySpawn(ref clone, prefab.transform.position, Quaternion.identity, Vector3.one, parent, worldPositionStays);
			}
			return TrySpawn(ref clone, transform.localPosition, transform.localRotation, transform.localScale, parent, worldPositionStays);
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.</summary>
		public bool TrySpawn(ref GameObject clone, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (prefab == null) { if (warnings == true) Debug.LogWarning("You're attempting to spawn from a pool with a null prefab", this); return false; }
			if (parent != null)
			{
				position = parent.InverseTransformPoint(position);
				rotation = Quaternion.Inverse(parent.rotation) * rotation;
			}
			return TrySpawn(ref clone, position, rotation, prefab.transform.localScale, parent, false);
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.</summary>
		public bool TrySpawn(ref GameObject clone)
		{
			if (prefab == null) { if (warnings == true) Debug.LogWarning("You're attempting to spawn from a pool with a null prefab", this); return false; }
			var transform = prefab.transform;
			return TrySpawn(ref clone, transform.localPosition, transform.localRotation, transform.localScale, null, false);
		}

		/// <summary>This will either spawn a previously despawned/preloaded clone, recycle one, create a new one, or return null.</summary>
		public bool TrySpawn(ref GameObject clone, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
			if (prefab != null)
			{
				// Spawn a previously despawned/preloaded clone?
				for (var i = despawnedClones.Count - 1; i >= 0; i--)
				{
					clone = despawnedClones[i];

					despawnedClones.RemoveAt(i);

					if (clone != null)
					{
						SpawnClone(clone, localPosition, localRotation, localScale, parent, worldPositionStays);

						return true;
					}

					if (warnings == true) Debug.LogWarning("This pool contained a null despawned clone, did you accidentally destroy it?", this);
				}

				// Make a new clone?
				if (capacity <= 0 || Total < capacity)
				{
					clone = CreateClone(localPosition, localRotation, localScale, parent, worldPositionStays);

					// Add clone to spawned list
					if (recycle == true)
					{
						spawnedClonesList.Add(clone);
					}
					else
					{
						spawnedClonesHashSet.Add(clone);
					}

					// Activate?
					if (strategy == StrategyType.ActivateAndDeactivate)
					{
						clone.SetActive(true);
					}

					// Notifications
					InvokeOnSpawn(clone);

					return true;
				}

				// Recycle?
				if (recycle == true && TryDespawnOldest(ref clone, false) == true)
				{
					SpawnClone(clone, localPosition, localRotation, localScale, parent, worldPositionStays);

					return true;
				}
			}
			else
			{
				if (warnings == true) Debug.LogWarning("You're attempting to spawn from a pool with a null prefab", this);
			}

			return false;
		}

		/// <summary>This will despawn the oldest prefab clone that is still spawned.</summary>
		[ContextMenu("Despawn Oldest")]
		public void DespawnOldest()
		{
			var clone = default(GameObject);

			TryDespawnOldest(ref clone, true);
		}

		private bool TryDespawnOldest(ref GameObject clone, bool registerDespawned)
		{
			MergeSpawnedClonesToList();

			// Loop through all spawnedClones from the front (oldest) until one is found
			while (spawnedClonesList.Count > 0)
			{
				clone = spawnedClonesList[0];

				spawnedClonesList.RemoveAt(0);

				if (clone != null)
				{
					DespawnNow(clone, registerDespawned);

					return true;
				}

				if (warnings == true) Debug.LogWarning("This pool contained a null spawned clone, did you accidentally destroy it?", this);
			}

			return false;
		}

		/// <summary>This method will despawn all currently spawned prefabs managed by this pool.</summary>
		[ContextMenu("Despawn All")]
		public void DespawnAll()
		{
			// Merge
			MergeSpawnedClonesToList();

			// Despawn
			for (var i = spawnedClonesList.Count - 1; i >= 0; i--)
			{
				var clone = spawnedClonesList[i];

				if (clone != null)
				{
					DespawnNow(clone);
				}
			}

			spawnedClonesList.Clear();

			// Clear all delays
			for (var i = delays.Count - 1; i >= 0; i--)
			{
				LeanClassPool<Delay>.Despawn(delays[i]);
			}

			delays.Clear();
		}

		/// <summary>This will either instantly despawn the specified gameObject, or delay despawn it after t seconds.</summary>
		public void Despawn(GameObject clone, float t = 0.0f)
		{
			if (clone != null)
			{
				// Delay the despawn?
				if (t > 0.0f)
				{
					DespawnWithDelay(clone, t);
				}
				// Despawn now?
				else
				{
					TryDespawn(clone);

					// If this clone was marked for delayed despawn, remove it
					for (var i = delays.Count - 1; i >= 0; i--)
					{
						var delay = delays[i];

						if (delay.Clone == clone)
						{
							delays.RemoveAt(i);
						}
					}
				}
			}
			else
			{
				if (warnings == true) Debug.LogWarning("You're attempting to despawn a null gameObject", this);
			}
		}

		/// <summary>This allows you to remove all references to the specified clone from this pool.
		/// A detached clone will act as a normal GameObject, requiring you to manually destroy or otherwise manage it.
		/// NOTE: If this clone has been despawned then it will still be parented to the pool.</summary>
		public void Detach(GameObject clone)
		{
			if (clone != null)
			{
				if (spawnedClonesHashSet.Remove(clone) == true || spawnedClonesList.Remove(clone) == true || despawnedClones.Remove(clone) == true)
				{
					// Remove the link between this clone and this pool if it hasn't already been
					LeanPool.Links.Remove(clone);

					// If this clone was marked for delayed despawn, remove it
					for (var i = delays.Count - 1; i >= 0; i--)
					{
						var delay = delays[i];

						if (delay.Clone == clone)
						{
							delays.RemoveAt(i);
						}
					}
				}
				else
				{
					if (warnings == true) Debug.LogWarning("You're attempting to detach a GameObject that wasn't spawned from this pool.", clone);
				}
			}
			else
			{
				if (warnings == true) Debug.LogWarning("You're attempting to detach a null GameObject", this);
			}
		}

		/// <summary>This method will create an additional prefab clone and add it to the despawned list.</summary>
		[ContextMenu("Preload One More")]
		public void PreloadOneMore()
		{
			if (prefab != null)
			{
				// Create clone
				var clone = CreateClone(Vector3.zero, Quaternion.identity, Vector3.one, null, false);

				// Add clone to despawned list
				despawnedClones.Add(clone);

				// Deactivate it
				if (strategy == StrategyType.ActivateAndDeactivate)
				{
					clone.SetActive(false);

					clone.transform.SetParent(transform, false);
				}
				else
				{
					clone.transform.SetParent(DeactivatedChild, false);
				}

				if (warnings == true && capacity > 0 && Total > capacity) Debug.LogWarning("You've preloaded more than the pool capacity, please verify you're preloading the intended amount.", this);
			}
			else
			{
				if (warnings == true) Debug.LogWarning("Attempting to preload a null prefab.", this);
			}
		}

		/// <summary>This will preload the pool based on the <b>Preload</b> setting.</summary>
		[ContextMenu("Preload All")]
		public void PreloadAll()
		{
			if (preload > 0)
			{
				if (prefab != null)
				{
					for (var i = Total; i < preload; i++)
					{
						PreloadOneMore();
					}
				}
				else if (warnings == true)
				{
					if (warnings == true) Debug.LogWarning("Attempting to preload a null prefab", this);
				}
			}
		}

		/// <summary>This will destroy all preloaded or despawned clones. This is useful if you've despawned more prefabs than you likely need, and want to free up some memory.</summary>
		[ContextMenu("Clean")]
		public void Clean()
		{
			for (var i = despawnedClones.Count - 1; i >= 0; i--)
			{
				DestroyImmediate(despawnedClones[i]);
			}

			despawnedClones.Clear();
		}

		/// <summary>This method will clear and fill the specified list with the specified clones from this pool.</summary>
		public void GetClones(List<GameObject> gameObjects, bool addSpawnedClones = true, bool addDespawnedClones = true)
		{
			if (gameObjects != null)
			{
				gameObjects.Clear();

				if (addSpawnedClones == true)
				{
					gameObjects.AddRange(spawnedClonesList);
					gameObjects.AddRange(spawnedClonesHashSet);
				}

				if (addDespawnedClones == true)
				{
					gameObjects.AddRange(despawnedClones);
				}
			}
		}

		protected virtual void Awake()
		{
			if (Application.isPlaying == true)
			{
				PreloadAll();

				if (persist == true)
				{
					DontDestroyOnLoad(this);
				}
			}
		}

		protected virtual void OnEnable()
		{
			instancesNode = Instances.AddLast(this);

			RegisterPrefab();
		}

		protected virtual void OnDisable()
		{
			UnregisterPrefab();

			Instances.Remove(instancesNode); instancesNode = null;
		}

		protected virtual void OnDestroy()
		{
			// If OnDestroy is called then the scene is likely changing, so we detach the spawned prefabs from the global links dictionary to prevent issues.
			foreach (var clone in spawnedClonesList)
			{
				if (clone != null)
				{
					LeanPool.Detach(clone, false);
				}
			}

			foreach (var clone in spawnedClonesHashSet)
			{
				if (clone != null)
				{
					LeanPool.Detach(clone, false);
				}
			}
		}

		protected virtual void Update()
		{
			// Decay the life of all delayed destruction calls
			for (var i = delays.Count - 1; i >= 0; i--)
			{
				var delay = delays[i];

				delay.Life -= Time.deltaTime;

				// Skip to next one?
				if (delay.Life > 0.0f)
				{
					continue;
				}

				// Remove and pool delay
				delays.RemoveAt(i); LeanClassPool<Delay>.Despawn(delay);

				// Finally despawn it after delay
				if (delay.Clone != null)
				{
					Despawn(delay.Clone);
				}
				else
				{
					if (warnings == true) Debug.LogWarning("Attempting to update the delayed destruction of a prefab clone that no longer exists, did you accidentally destroy it?", this);
				}
			}
		}

		private void RegisterPrefab()
		{
			if (prefab != null)
			{
				var existingPool = default(LeanGameObjectPool);

				if (prefabMap.TryGetValue(prefab, out existingPool) == true)
				{
					Debug.LogWarning("You have multiple pools managing the same prefab (" + prefab.name + ").", existingPool);
				}
				else
				{
					prefabMap.Add(prefab, this);
				}
			}
		}

		private void UnregisterPrefab()
		{
			// Skip actually null prefabs, but allow destroyed prefabs
			if (Equals(prefab, null) == true)
			{
				return;
			}

			var existingPool = default(LeanGameObjectPool);

			if (prefabMap.TryGetValue(prefab, out existingPool) == true && existingPool == this)
			{
				prefabMap.Remove(prefab);
			}
		}

		private void DespawnWithDelay(GameObject clone, float t)
		{
			// If this object is already marked for delayed despawn, update the time and return
			for (var i = delays.Count - 1; i >= 0; i--)
			{
				var delay = delays[i];

				if (delay.Clone == clone)
				{
					if (t < delay.Life)
					{
						delay.Life = t;
					}

					return;
				}
			}

			// Create delay
			var newDelay = LeanClassPool<Delay>.Spawn() ?? new Delay();

			newDelay.Clone = clone;
			newDelay.Life  = t;

			delays.Add(newDelay);
		}

		private void TryDespawn(GameObject clone)
		{
			if (spawnedClonesHashSet.Remove(clone) == true || spawnedClonesList.Remove(clone) == true)
			{
				DespawnNow(clone);
			}
			else
			{
				if (warnings == true) Debug.LogWarning("You're attempting to despawn a GameObject that wasn't spawned from this pool, make sure your Spawn and Despawn calls match.", clone);
			}
		}

		private void DespawnNow(GameObject clone, bool register = true)
		{
			// Add clone to despawned list
			if (register == true)
			{
				despawnedClones.Add(clone);
			}

			// Messages?
			InvokeOnDespawn(clone);

			// Deactivate it
			if (strategy == StrategyType.ActivateAndDeactivate)
			{
				clone.SetActive(false);

				clone.transform.SetParent(transform, false);
			}
			else
			{
				clone.transform.SetParent(DeactivatedChild, false);
			}
		}

		private GameObject CreateClone(Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
			var clone = DoInstantiate(prefab, localPosition, localRotation, localScale, parent, worldPositionStays);

			if (stamp == true)
			{
				clone.name = prefab.name + " " + Total;
			}
			else
			{
				clone.name = prefab.name;
			}

			return clone;
		}

		private GameObject DoInstantiate(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
#if UNITY_EDITOR
			if (Application.isPlaying == false && UnityEditor.PrefabUtility.IsPartOfRegularPrefab(prefab) == true)
			{
				if (worldPositionStays == true)
				{
					return (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
				}
				else
				{
					var clone = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);

					clone.transform.localPosition = localPosition;
					clone.transform.localRotation = localRotation;
					clone.transform.localScale    = localScale;

					return clone;
				}
			}
#endif

			if (worldPositionStays == true)
			{
				return Instantiate(prefab, parent, true);
			}
			else
			{
				var clone = Instantiate(prefab, localPosition, localRotation, parent);

				clone.transform.localPosition = localPosition;
				clone.transform.localRotation = localRotation;
				clone.transform.localScale    = localScale;

				return clone;
			}
		}

		private void SpawnClone(GameObject clone, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
			// Register
			if (recycle == true)
			{
				spawnedClonesList.Add(clone);
			}
			else
			{
				spawnedClonesHashSet.Add(clone);
			}

			// Update transform
			var cloneTransform = clone.transform;

			cloneTransform.SetParent(null, false);

			cloneTransform.localPosition = localPosition;
			cloneTransform.localRotation = localRotation;
			cloneTransform.localScale    = localScale;

			cloneTransform.SetParent(parent, worldPositionStays);

			// Make sure it's in the current scene
			if (parent == null)
			{
				SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());
			}

			// Activate
			if (strategy == StrategyType.ActivateAndDeactivate)
			{
				clone.SetActive(true);
			}

			// Notifications
			InvokeOnSpawn(clone);
		}

		private void InvokeOnSpawn(GameObject clone)
		{
			switch (notification)
			{
				case NotificationType.SendMessage: clone.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.BroadcastMessage: clone.BroadcastMessage("OnSpawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.IPoolable: clone.GetComponents(tempPoolables); for (var i = tempPoolables.Count - 1; i >= 0; i--) tempPoolables[i].OnSpawn(); break;
				case NotificationType.BroadcastIPoolable: clone.GetComponentsInChildren(tempPoolables); for (var i = tempPoolables.Count - 1; i >= 0; i--) tempPoolables[i].OnSpawn(); break;
			}
		}

		private void InvokeOnDespawn(GameObject clone)
		{
			switch (notification)
			{
				case NotificationType.SendMessage: clone.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.BroadcastMessage: clone.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.IPoolable: clone.GetComponents(tempPoolables); for (var i = tempPoolables.Count - 1; i >= 0; i--) tempPoolables[i].OnDespawn(); break;
				case NotificationType.BroadcastIPoolable: clone.GetComponentsInChildren(tempPoolables); for (var i = tempPoolables.Count - 1; i >= 0; i--) tempPoolables[i].OnDespawn(); break;
			}
		}

		private void MergeSpawnedClonesToList()
		{
			if (spawnedClonesHashSet.Count > 0)
			{
				spawnedClonesList.AddRange(spawnedClonesHashSet);

				spawnedClonesHashSet.Clear();
			}
		}

		public void OnBeforeSerialize()
		{
			MergeSpawnedClonesToList();
		}

		public void OnAfterDeserialize()
		{
			if (recycle == false)
			{
				for (var i = spawnedClonesList.Count - 1; i >= 0; i--)
				{
					var clone = spawnedClonesList[i];

					spawnedClonesHashSet.Add(clone);
				}

				spawnedClonesList.Clear();
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Pool.Editor
{
	using UnityEditor;
	using TARGET = LeanGameObjectPool;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanGameObjectPool_Editor : CwEditor
	{
		private static List<GameObject> tempClones = new List<GameObject>();

		[System.NonSerialized] TARGET tgt; [System.NonSerialized] TARGET[] tgts;

		protected override void OnInspector()
		{
			GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Prefab == null));
				if (Draw("prefab", "The prefab this pool controls.") == true)
				{
					Each(tgts, t => { t.Prefab = (GameObject)serializedObject.FindProperty("prefab").objectReferenceValue; }, true);
				}
			EndError();
			Draw("notification", "If you need to perform a special action when a prefab is spawned or despawned, then this allows you to control how that action is performed. None = If you use this then you must rely on the OnEnable and OnDisable messages. SendMessage = The prefab clone is sent the OnSpawn and OnDespawn messages. BroadcastMessage = The prefab clone and all its children are sent the OnSpawn and OnDespawn messages. IPoolable = The prefab clone's components implementing IPoolable are called. Broadcast IPoolable = The prefab clone and all its child components implementing IPoolable are called.");
			Draw("strategy", "This allows you to control how spawned/despawned GameObjects will be handled. The DeactivateViaHierarchy mode should be used if you need to maintain your prefab's de/activation state.\n\nActivateAndDeactivate = Despawned clones will be deactivated and placed under this GameObject.\n\nDeactivateViaHierarchy = Despawned clones will be placed under a deactivated GameObject and left alone.");
			Draw("preload", "Should this pool preload some clones?");
			Draw("capacity", "Should this pool have a maximum amount of spawnable clones?");
			Draw("recycle", "If the pool reaches capacity, should new spawns force older ones to despawn?");
			Draw("persist", "Should this pool be marked as DontDestroyOnLoad?");
			Draw("stamp", "Should the spawned clones have their clone index appended to their name?");
			Draw("warnings", "Should detected issues be output to the console?");

			Separator();

			BeginDisabled();
				DrawClones("Spawned", true, false, "notification");
				DrawClones("Despawned", false, true, "strategy");
				DrawClones("Total", true, true, "preload");
			EndDisabled();

			if (Application.isPlaying == false)
			{
				if (Any(tgts, t => t.DespawnedClonesMatch == false))
				{
					Warning("Your preloaded clones no longer match the Prefab.");
				}
			}
		}

		private void DrawClones(string title, bool spawned, bool despawned, string propertyName)
		{
			var property = serializedObject.FindProperty(propertyName);
			var rect     = EditorGUILayout.BeginVertical(); EditorGUILayout.LabelField(string.Empty, GUILayout.Height(EditorGUI.GetPropertyHeight(property))); EditorGUILayout.EndVertical();
			var rectF    = rect; rectF.height = 16;

			tgt.GetClones(tempClones, spawned, despawned);

			property.isExpanded = EditorGUI.Foldout(rectF, property.isExpanded, GUIContent.none);

			UnityEditor.EditorGUI.IntField(rect, title, tempClones.Count);

			if (property.isExpanded == true)
			{
				foreach (var clone in tempClones)
				{
					EditorGUILayout.ObjectField(GUIContent.none, clone, typeof(GameObject), true);
				}
			}
		}

		[UnityEditor.MenuItem("GameObject/Lean/Pool", false, 1)]
		private static void CreateLocalization()
		{
			var gameObject = new GameObject(typeof(LeanGameObjectPool).Name);

			UnityEditor.Undo.RegisterCreatedObjectUndo(gameObject, "Create LeanGameObjectPool");

			gameObject.AddComponent<LeanGameObjectPool>();

			UnityEditor.Selection.activeGameObject = gameObject;
		}
	}
}
#endif