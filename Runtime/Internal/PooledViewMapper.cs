using System;
using UniMob.UI.Internal.Pooling;
using UnityEngine;

namespace UniMob.UI.Internal
{
    public sealed class PooledViewMapper : ViewMapperBase
    {
        private readonly Func<Transform> _parentSelector;
        private readonly bool _worldPositionStays;

        public PooledViewMapper(Transform parent, bool worldPositionStays = false)
            : this(() => parent, worldPositionStays)
        {
        }

        public PooledViewMapper(Func<Transform> parentSelector, bool worldPositionStays)
        {
            _parentSelector = parentSelector;
            _worldPositionStays = worldPositionStays;
        }

        protected override IView ResolveView(IViewState state)
        {
            var (prefab, viewRef) = ViewContext.Loader.LoadViewPrefab(state);
            var view = GameObjectPool
                .Instantiate(prefab.gameObject, _parentSelector.Invoke(), _worldPositionStays)
                .GetComponent<IView>();
            view.gameObject.name = prefab.gameObject.name;
            view.rectTransform.anchoredPosition = Vector2.zero;
            view.ViewReference = viewRef;
            return view;
        }

        protected override void RecycleView(IView view)
        {
            GameObjectPool.Recycle(view.gameObject, true);
        }
    }
}