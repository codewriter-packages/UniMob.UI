using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniMob.UI.Internal.ViewLoaders
{
    internal class AddressableViewLoader : IViewLoader
    {
        private static readonly Dictionary<object, string> RuntimeKeyToPath = new Dictionary<object, string>();

        public IView LoadViewPrefab(WidgetViewReference viewReference)
        {
            if (viewReference.Type != WidgetViewReferenceType.Addressable)
            {
                return null;
            }

            var path = viewReference.Path ?? GetAddressableAssetPath(viewReference);

            if (!UniMobAddressablesPreloadHandle.TryGetPrefab(path, out var prefab))
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
    }
}