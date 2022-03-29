using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class ViewState : State, IViewState
    {
        private LifetimeController _mountLifetimeController;
        
        [Atom] public override WidgetSize Size => CalculateSize();

        public abstract WidgetViewReference View { get; }

        public sealed override IViewState InnerViewState => this;

        public Lifetime MountLifetime
        {
            get
            {
                if (_mountLifetimeController == null)
                {
                    _mountLifetimeController = new LifetimeController();
                }

                return _mountLifetimeController.Lifetime;
            }
        }

        public virtual void DidViewMount(IView view)
        {
            Assert.IsNull(Atom.CurrentScope);
        }

        public virtual void DidViewUnmount(IView view)
        {
            Assert.IsNull(Atom.CurrentScope);
            
            _mountLifetimeController?.Dispose();
        }

        public virtual WidgetSize CalculateSize()
        {
            var (prefab, viewRef) = UniMobViewContext.Loader.LoadViewPrefab(View);
            viewRef.LinkAtomToScope();
            var size = prefab.rectTransform.sizeDelta;

            return new WidgetSize(
                size.x > 0 ? size.x : 0,
                size.y > 0 ? size.y : 0,
                size.x > 0 ? size.x : float.PositiveInfinity,
                size.y > 0 ? size.y : float.PositiveInfinity
            );
        }
    }
}