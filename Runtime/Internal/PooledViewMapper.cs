using UniMob.UI.Internal.Pooling;
using UnityEngine;

namespace UniMob.UI.Internal
{
    public sealed class PooledViewMapper : ViewMapperBase
    {
        private readonly Transform _parent;

        public PooledViewMapper(Transform parent, bool link = true) : base(link)
        {
            _parent = parent;
        }

        protected override IView ResolveView(WidgetViewReference viewReference)
        {
            using (Atom.NoWatch)
            {
                var prefab = UniMobViewContext.Loader.LoadViewPrefab(viewReference);
                var view = ViewPool
                    .Instantiate(prefab.gameObject, _parent)
                    .GetComponent<IView>();
#if UNITY_EDITOR
                view.gameObject.name = prefab.gameObject.name;
#endif
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
                ViewPool.Recycle(view.gameObject);
            }
        }
    }
}