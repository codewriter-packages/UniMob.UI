using System.Collections.Generic;
using UnityEngine;

namespace UniMob.UI.Internal.ViewLoaders
{
    public class PrefabViewLoader : IViewLoader
    {
        private readonly Dictionary<GameObject, IView> _viewPrefabCache = new Dictionary<GameObject, IView>();

        public (IView, WidgetViewReference) LoadViewPrefab(WidgetViewReference viewReference)
        {
            if (viewReference.Type != WidgetViewReferenceType.Prefab)
            {
                return (null, default);
            }

            var prefab = viewReference.Prefab;

            if (_viewPrefabCache.TryGetValue(prefab, out var view))
            {
                return (view, viewReference);
            }

            if (prefab == null)
            {
                Debug.LogError($"Prefab is null");
                return (null, default);
            }

            view = prefab.GetComponent<IView>();
            if (view == null)
            {
                Debug.LogError($"Failed to get IView from prefab '{prefab.name}'. Missing view component?");
                return (null, default);
            }

            _viewPrefabCache.Add(prefab, view);

            return (view, viewReference);
        }
    }
}