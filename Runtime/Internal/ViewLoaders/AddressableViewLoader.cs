using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniMob.UI.Internal.ViewLoaders
{
    internal class AddressableViewLoader : IViewLoader
    {
        private static readonly List<IUniMobAddressablesLoader> Loaders = new List<IUniMobAddressablesLoader>();
        private static readonly Dictionary<object, string> RuntimeKeyToPath = new Dictionary<object, string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            if (Loaders.Count > 0)
            {
                Loaders.Clear();

                Debug.LogError(
                    "[UniMob] You must Dispose all UniMobAddressablesPreloadHandle when application is closed");
            }
        }

        [PublicAPI]
        public static void RegisterAddressablesLoader(Lifetime lifetime, IUniMobAddressablesLoader loader)
        {
            if (lifetime.IsDisposed)
            {
                return;
            }

            Loaders.Add(loader);
            lifetime.Register(() => Loaders.Remove(loader));
        }

        public IView LoadViewPrefab(WidgetViewReference viewReference)
        {
            if (viewReference.Type != WidgetViewReferenceType.Addressable)
            {
                return null;
            }

            var path = viewReference.Path ?? GetAddressableAssetPath(viewReference);

            if (!TryGetAddressablePrefab(path, out var prefab))
            {
                Debug.LogError($"Failed to resolve addressable '{path}'. Not preloaded?\n" +
                               $"Addressable prefabs must be preloaded with {nameof(UniMobAddressablesPreloadHandle)}");

                return null;
            }

            if (prefab == null)
            {
                Debug.LogError($"Failed to load addressable '{path}'. Invalid path?");
                return null;
            }

            if (!prefab.TryGetComponent(out IView view))
            {
                Debug.LogError($"Failed to get IView from addressable '{path}'. Missing view component?");
                return null;
            }

            return view;
        }

        private static string GetAddressableAssetPath(WidgetViewReference viewReference)
        {
            var key = viewReference.Reference.RuntimeKey;

            if (RuntimeKeyToPath.TryGetValue(key, out var path))
            {
                return path;
            }

            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key, typeof(GameObject), out var locations))
                {
                    foreach (var location in locations)
                    {
                        RuntimeKeyToPath[key] = location.PrimaryKey;

                        return location.PrimaryKey;
                    }
                }
            }

            throw new InvalidOperationException($"Failed to resolve addressable asset path from '{key}'");
        }

        private static bool TryGetAddressablePrefab(string path, out GameObject prefab)
        {
            foreach (var handle in Loaders)
            {
                if (handle.TryGetPrefab(path, out prefab))
                {
                    return true;
                }
            }

            prefab = default;
            return false;
        }
    }

    public static class AddressableViewLoaderInternal
    {
        [PublicAPI]
        public static void RegisterAddressablesLoader(Lifetime lifetime, IUniMobAddressablesLoader loader)
        {
            AddressableViewLoader.RegisterAddressablesLoader(lifetime, loader);
        }
    }
}