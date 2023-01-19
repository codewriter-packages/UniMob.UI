using System;
using System.Collections.Generic;
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
        private readonly List<AsyncOperationHandle> _operations;
        private readonly Dictionary<string, GameObject> _prefabs;

        private UniMobAddressablesPreloadHandle(object key)
        {
            _key = key;
            _operations = new List<AsyncOperationHandle>();
            _prefabs = new Dictionary<string, GameObject>();

            Handles.Add(this);
        }

        public async Task LoadAsync()
        {
            var loadLocationsOperation = Addressables.LoadResourceLocationsAsync(_key, typeof(GameObject));
            _operations.Add(loadLocationsOperation);

            var locations = await loadLocationsOperation.Task;
            var queuedTasks = new List<Task>(locations.Count);

            foreach (var location in locations)
            {
                var task = LoadAsset(location);

                queuedTasks.Add(task);
            }

            await Task.WhenAll(queuedTasks);
            queuedTasks.Clear();
        }

        private async Task LoadAsset(IResourceLocation location)
        {
            var loadOperation = Addressables.LoadAssetAsync<GameObject>(location);
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