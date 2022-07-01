using System.Collections.Generic;
using UnityEngine;

namespace UniMob.UI.Internal.ViewLoaders
{
    public class PrefabViewLoader : IViewLoader
    {
        private readonly Dictionary<GameObject, IView> _viewPrefabCache = new Dictionary<GameObject, IView>();

        public IView LoadViewPrefab(WidgetViewReference viewReference)
        {
            if (viewReference.Type != WidgetViewReferenceType.Prefab)
            {
                return null;
            }

            var prefab = viewReference.Prefab;

            if (_viewPrefabCache.TryGetValue(prefab, out var view))
            {
                return view;
            }

            if (prefab == null)
            {
                Debug.LogError($"Prefab is null");
                return null;
            }

            view = prefab.GetComponent<IView>();
            if (view == null)
            {
                Debug.LogError($"Failed to get IView from prefab '{prefab.name}'. Missing view component?");
                return null;
            }

            _viewPrefabCache.Add(prefab, view);

            return view;
        }
    }
}