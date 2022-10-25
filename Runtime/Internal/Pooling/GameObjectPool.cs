using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UniMob.UI.Internal.Pooling
{
    internal static class GameObjectPool
    {
        private static readonly Dictionary<int, Pool> Pools = new Dictionary<int, Pool>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            Pools.Clear();
        }

        public static Pool GetPool([NotNull] GameObject prefab)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            var prefabID = prefab.GetInstanceID();

            Pools.TryGetValue(prefabID, out var pool);

            if (pool != null)
                return pool;

            pool = new GameObject("Pool").AddComponent<Pool>();
            pool.Init(prefab);
            Pools.Add(prefabID, pool);

            return pool;
        }

        public static GameObject Instantiate([NotNull] GameObject prefab, Transform parent = null,
            bool worldPositionStays = true)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            var prefabID = prefab.GetInstanceID();
            var pool = GetPool(prefab);
            var obj = pool.Get(parent, worldPositionStays);

            var poolID = obj.GetComponent<PoolID>() ?? obj.AddComponent<PoolID>();
            poolID.PrefabInstanceID = prefabID;

            return obj;
        }

        public static void Recycle(GameObject obj, bool deactivate = true, bool worldPositionStays = false,
            bool resetParent = true)
        {
            if (obj == null)
            {
                Debug.LogWarning("[GameObjectPool] object is null");
                return;
            }

            var poolID = obj.GetComponent<PoolID>();
            if (poolID == null)
            {
                Debug.LogError("[GameObjectPool] PoolID component not attached", obj);
                Object.Destroy(obj);
                return;
            }

            if (poolID.PrefabInstanceID == 0 || poolID.ObjectDestroyed)
                return;

            if (!Pools.TryGetValue(poolID.PrefabInstanceID, out var pool))
            {
                Debug.LogError("[GameObjectPool] pool for object not exists", obj);
                Object.Destroy(obj);
                return;
            }

            poolID.PrefabInstanceID = 0;
            pool.Return(obj, deactivate, worldPositionStays, resetParent);
        }

        public sealed class Pool : MonoBehaviour
        {
            private readonly Queue<GameObject> _stack = new Queue<GameObject>();
            private GameObject _prefab;
            private string _prefabName;
            private bool _poolDestroyed;

            private void Start()
            {
                DontDestroyOnLoad(this);
                DontDestroyOnLoad(gameObject);
            }

            private void OnDestroy()
            {
                _poolDestroyed = true;
            }

            public void Init([NotNull] GameObject prefab)
            {
                if (prefab == null) throw new ArgumentNullException(nameof(prefab));

                _prefab = prefab;
                _prefabName = prefab.name;

                EditorUpdateName();
            }

            public GameObject Get(Transform parent = null, bool worldPositionStays = true)
            {
                if (_poolDestroyed)
                {
                    throw new InvalidOperationException("Cannot get object from destroyed pool");
                }

                GameObject obj;
                if (_stack.Count > 0)
                {
                    obj = _stack.Dequeue();
                    obj.transform.SetParent(parent, worldPositionStays);
                }
                else
                {
                    obj = Object.Instantiate(_prefab, parent, worldPositionStays);
                }

                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                }

                EditorUpdateName();

                return obj;
            }

            public void Return(GameObject instance, bool deactivate, bool worldPositionStays = false,
                bool resetParent = true)
            {
                if (_poolDestroyed)
                    return;

                if (instance == null)
                    return;

                if (deactivate)
                    instance.SetActive(false);

                if (resetParent)
                    instance.transform.SetParent(transform, worldPositionStays);

                _stack.Enqueue(instance);

                EditorUpdateName();
            }

            [Conditional("UNITY_EDITOR")]
            private void EditorUpdateName()
            {
#if UNITY_EDITOR
                name = $"{_prefabName} Pool ({_stack.Count})";
#endif
            }
        }
    }
}