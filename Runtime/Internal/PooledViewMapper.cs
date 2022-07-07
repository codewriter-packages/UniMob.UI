using System;
using UniMob.UI.Internal.Pooling;
using UnityEngine;

namespace UniMob.UI.Internal
{
    public sealed class PooledViewMapper : ViewMapperBase
    {
        private readonly Func<Transform> _parentSelector;
        private readonly bool _worldPositionStays;

        public PooledViewMapper(Transform parent, bool worldPositionStays = false, bool link = true)
            : this(() => parent, worldPositionStays, link)
        {
        }

        public PooledViewMapper(Func<Transform> parentSelector, bool worldPositionStays, bool link) 
            : base(link)
        {
            _parentSelector = parentSelector;
            _worldPositionStays = worldPositionStays;
        }

        protected override IView ResolveView(WidgetViewReference viewReference)
        {
            using (Atom.NoWatch)
            {
                var prefab = UniMobViewContext.Loader.LoadViewPrefab(viewReference);
                var view = GameObjectPool
                    .Instantiate(prefab.gameObject, _parentSelector.Invoke(), _worldPositionStays)
                    .GetComponent<IView>();
                view.gameObject.name = prefab.gameObject.name;
                view.rectTransform.anchoredPosition = Vector2.zero;
                return view;
            }
        }

        protected override void RecycleView(IView view)
        {
            if (view.IsDestroyed)
                return;

            using (Atom.NoWatch)
            {
                GameObjectPool.Recycle(view.gameObject, true);
            }
        }
    }
}