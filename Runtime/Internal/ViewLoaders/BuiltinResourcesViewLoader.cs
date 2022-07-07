using System.Collections.Generic;
using UnityEngine;

namespace UniMob.UI.Internal.ViewLoaders
{
    internal class BuiltinResourcesViewLoader : IViewLoader
    {
        private readonly Dictionary<string, IView> _viewPrefabCache = new Dictionary<string, IView>();

        public IView LoadViewPrefab(WidgetViewReference viewReference)
        {
            if (viewReference.Type != WidgetViewReferenceType.Resource)
            {
                return null;
            }

            var path = viewReference.Path;

            if (_viewPrefabCache.TryGetValue(path, out var view))
            {
                return view;
            }

            var prefab = Resources.Load(path) as GameObject;
            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab '{path}'. Invalid path?");
                return null;
            }

            view = prefab.GetComponent<IView>();
            if (view == null)
            {
                Debug.LogError($"Failed to get IView from prefab '{path}'. Missing view component?");
                return null;
            }

            _viewPrefabCache.Add(path, view);

            return view;
        }
    }
}