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

        public static Pool GetPool([NotNull] GameObject prefab)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            var prefabID = prefab.GetInstanceID();

            Pools.TryGetValue(prefabID, out var pool);

            if (pool != null)
                return pool;

            pool = new Pool(prefab);
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

            if (poolID.PrefabInstanceID == 0)
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

        public sealed class Pool
        {
            private readonly Stack<GameObject> _stack = new Stack<GameObject>();
            private readonly GameObject _prefab;
            private readonly Transform _root;

            public int CountOfObjectsInPool => _stack.Count;
            public GameObject Prefab => _prefab;
            public Transform Root => _root;

            public Pool([NotNull] GameObject prefab)
            {
                if (prefab == null) throw new ArgumentNullException(nameof(prefab));

                _prefab = prefab;
                _root = new GameObject().transform;
                Object.DontDestroyOnLoad(_root);

                EditorUpdateName();
            }

            public GameObject Get(Transform parent = null, bool worldPositionStays = true)
            {
                GameObject obj;
                if (_stack.Count > 0)
                {
                    obj = _stack.Pop();
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

            public void Return(GameObject gameObject, bool deactivate, bool worldPositionStays = false,
                bool resetParent = true)
            {
                if (gameObject == null)
                    return;

                if (deactivate)
                    gameObject.SetActive(false);

                if (resetParent)
                    gameObject.transform.SetParent(_root, worldPositionStays);

                _stack.Push(gameObject);

                EditorUpdateName();
            }

            [Conditional("UNITY_EDITOR")]
            private void EditorUpdateName()
            {
#if UNITY_EDITOR
                _root.name = $"{Prefab.name} Pool ({CountOfObjectsInPool})";
#endif
            }
        }
    }
}