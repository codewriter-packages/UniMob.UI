using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Debug = UnityEngine.Debug;

namespace UniMob.UI
{
    public class UniMobAddressablesPreloadHandle : IDisposable
    {
        private static readonly List<UniMobAddressablesPreloadHandle> Handles =
            new List<UniMobAddressablesPreloadHandle>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            if (Handles.Count > 0)
            {
                Debug.LogError(
                    "[UniMob] You must Dispose all UniMobAddressablesPreloadHandle when application is closed");
            }

            Handles.Clear();
        }

        private readonly object _key;
        private readonly int _loadGroupSize;
        private readonly List<AsyncOperationHandle> _operations;
        private readonly Dictionary<string, GameObject> _prefabs;

        private UniMobAddressablesPreloadHandle(object key, int loadGroupSize = 25)
        {
            _key = key;
            _loadGroupSize = loadGroupSize;
            _operations = new List<AsyncOperationHandle>();
            _prefabs = new Dictionary<string, GameObject>();

            Handles.Add(this);
        }

        public async Task LoadAsync()
        {
            var sw = Stopwatch.StartNew();

            var loadLocationsOperation = Addressables.LoadResourceLocationsAsync(_key, typeof(GameObject));
            _operations.Add(loadLocationsOperation);

            var locations = await loadLocationsOperation.Task;
            var queuedTasks = new List<Task>();

            foreach (var location in locations)
            {
                var task = LoadAsset(location);

                queuedTasks.Add(task);

                if (queuedTasks.Count > _loadGroupSize)
                {
                    await Task.WhenAll(queuedTasks);
                    queuedTasks.Clear();
                }
            }

            await Task.WhenAll(queuedTasks);
            queuedTasks.Clear();

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                Debug.Log($"Preload '{_key}' addressable assets in {sw.ElapsedMilliseconds} ms");
            }
        }

        private async Task LoadAsset(IResourceLocation location)
        {
            var loadOperation = Addressables.LoadAssetAsync<GameObject>(location.PrimaryKey);
            _operations.Add(loadOperation);

            var prefab = await loadOperation.Task;
            _prefabs[location.PrimaryKey] = prefab;
        }

        public void Dispose()
        {
            Handles.Remove(this);

            foreach (var operation in _operations)
            {
                Addressables.Release(operation);
            }
        }

        public static bool TryGetPrefab(string path, out GameObject prefab)
        {
            foreach (var handle in Handles)
            {
                if (handle._prefabs.TryGetValue(path, out prefab))
                {
                    return true;
                }
            }

            prefab = default;
            return false;
        }

        public static UniMobAddressablesPreloadHandle Create(object key)
        {
            return new UniMobAddressablesPreloadHandle(key);
        }
    }
}