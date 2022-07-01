using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniMob.UI.Internal.ViewLoaders
{
    internal class AddressableViewLoader : IViewLoader
    {
        private readonly Dictionary<string, IView> _viewPrefabCache = new Dictionary<string, IView>();

        public IView LoadViewPrefab(WidgetViewReference viewReference)
        {
            if (viewReference.Type != WidgetViewReferenceType.Addressable)
            {
                return null;
            }

            var path = viewReference.Path;

            var identifier = path ?? viewReference.Reference.RuntimeKey.ToString();

            if (_viewPrefabCache.TryGetValue(identifier, out var cachedView))
            {
                return cachedView;
            }

            var op = path != null
                ? Addressables.LoadAssetAsync<GameObject>(path)
                : viewReference.Reference.LoadAssetAsync<GameObject>();

            var prefab = op.WaitForCompletion();

            if (prefab == null)
            {
                Debug.LogError($"Failed to load addressable '{identifier}'. Invalid path?");
                return null;
            }

            var view = prefab.GetComponent<IView>();
            if (view == null)
            {
                Debug.LogError($"Failed to get IView from addressable '{identifier}'. Missing view component?");
                return null;
            }

            _viewPrefabCache.Add(identifier, view);

            return view;
        }
    }
}