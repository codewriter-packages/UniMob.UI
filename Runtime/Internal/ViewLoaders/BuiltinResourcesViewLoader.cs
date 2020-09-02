using System.Collections.Generic;
using UnityEngine;

namespace UniMob.UI.Internal.ViewLoaders
{
    internal class BuiltinResourcesViewLoader : IViewLoader
    {
        private readonly Dictionary<string, IView> _viewPrefabCache = new Dictionary<string, IView>();

        public (IView, WidgetViewReference) LoadViewPrefab(IViewState viewState)
        {
            var viewReference = viewState.View;

            if (viewReference.Type != WidgetViewReferenceType.Resource)
            {
                return (null, default);
            }

            var path = viewReference.Path;

            if (_viewPrefabCache.TryGetValue(path, out var view))
            {
                return (view, viewReference);
            }

            var prefab = Resources.Load(path) as GameObject;
            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab '{path}' for '{viewState.GetType().Name}'. " +
                               "Invalid path?");
                return (null, default);
            }

            view = prefab.GetComponent<IView>();
            if (view == null)
            {
                Debug.LogError($"Failed to get IView from prefab '{path}' for '{viewState.GetType().Name}'. " +
                               "Missing view component?");
                return (null, default);
            }

            _viewPrefabCache.Add(path, view);

            return (view, viewReference);
        }
    }
}