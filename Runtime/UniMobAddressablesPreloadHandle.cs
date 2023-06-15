using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniMob.UI.Internal.ViewLoaders;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UniMob.UI
{
    public interface IUniMobAddressablesLoader
    {
        bool TryGetPrefab(string path, out GameObject prefab);
    }

    public class UniMobAddressablesPreloadHandle : IUniMobAddressablesLoader, IDisposable
    {
        private readonly object _key;
        private readonly LifetimeController _lifetimeController;
        private readonly Dictionary<string, GameObject> _prefabs;

        public Lifetime Lifetime => _lifetimeController.Lifetime;

        private UniMobAddressablesPreloadHandle(object key)
        {
            _key = key;
            _lifetimeController = new LifetimeController();
            _prefabs = new Dictionary<string, GameObject>();
        }

        public bool TryGetPrefab(string path, out GameObject prefab)
        {
            return _prefabs.TryGetValue(path, out prefab);
        }

        public async Task LoadAsync()
        {
            var loadLocationsOperation = Addressables.LoadResourceLocationsAsync(_key, typeof(GameObject));
            Lifetime.Register(() => Addressables.Release(loadLocationsOperation));

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
            Lifetime.Register(() => Addressables.Release(loadOperation));

            var prefab = await loadOperation.Task;
            _prefabs[location.PrimaryKey] = prefab;
        }

        public void Dispose()
        {
            _lifetimeController.Dispose();
        }

        public static UniMobAddressablesPreloadHandle Create(object key)
        {
            var handle = new UniMobAddressablesPreloadHandle(key);

            AddressableViewLoader.RegisterAddressablesLoader(handle.Lifetime, handle);

            return handle;
        }
    }
}