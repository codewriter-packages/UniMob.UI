using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniMob.UI.Internal.ViewLoaders
{
    internal class AddressableViewLoader : IViewLoader
    {
        private readonly Dictionary<string, IView> _viewPrefabCache = new Dictionary<string, IView>();

        private readonly Dictionary<string, Data> _loaders = new Dictionary<string, Data>();

        private class Data
        {
            public MutableAtom<WidgetViewReference> ViewRef;
        }

        private IView _loadingViewPrefab;

        public (IView, WidgetViewReference) LoadViewPrefab(IViewState viewState)
        {
            var viewReference = viewState.View;

            if (viewReference.Type != WidgetViewReferenceType.Addressable)
            {
                return (null, default);
            }

            var path = viewReference.Path;
            
            var identifier = path ?? viewReference.Reference.RuntimeKey.ToString();

            if (_viewPrefabCache.TryGetValue(identifier, out var cachedView))
            {
                return (cachedView, viewReference);
            }

            if (!_loaders.TryGetValue(identifier, out var data))
            {
                var op = path != null
                    ? Addressables.LoadAssetAsync<GameObject>(path)
                    : viewReference.Reference.LoadAssetAsync<GameObject>();
                var tempReference = Atom.Value($"temp loading {identifier}",
                    WidgetViewReference.Resource("ADDR__loading__"));

                data = new Data
                {
                    ViewRef = tempReference,
                };

                _loaders.Add(identifier, data);

                op.Completed += handle =>
                {
                    if (handle.Result == null)
                    {
                        Debug.LogError($"Failed to load addressable '{identifier}' for '{viewState.GetType().Name}'. " +
                                       "Invalid path?");
                        return;
                    }

                    var prefab = handle.Result;
                    var view = prefab.GetComponent<IView>();
                    if (view == null)
                    {
                        Debug.LogError($"Failed to get IView from addressable '{identifier}' for '{viewState.GetType().Name}'. " +
                                       "Missing view component?");
                        return;
                    }

                    _viewPrefabCache.Add(identifier, view);
                    tempReference.Value = WidgetViewReference.Resource("ADDR__loaded__");
                };
            }

            if (_loadingViewPrefab == null)
            {
                var go = new GameObject(nameof(AddressableLoadingView),
                    typeof(RectTransform), typeof(AddressableLoadingView));
                Object.DontDestroyOnLoad(go);

                _loadingViewPrefab = go.GetComponent<IView>();
                _loadingViewPrefab.rectTransform.sizeDelta = Vector2.one;
            }

            return (_loadingViewPrefab, new WidgetViewReference(data.ViewRef));
        }
    }
}