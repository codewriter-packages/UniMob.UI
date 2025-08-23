using UnityEngine;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class ViewState : State, IViewState
    {
        private LifetimeController _mountLifetimeController;

        [Atom] public override WidgetSize Size => CalculateSize();

        Vector2 IViewState.ViewMaxSize => ViewMaxSize;

        [Atom] private Vector2 ViewMaxSize
        {
            get
            {
                var prefab = UniMobViewContext.Loader.LoadViewPrefab(View);
                var size = Vector2.Max(prefab.rectTransform.sizeDelta, Vector2.zero);

                var (width, height) = (size.x, size.y);

                // zero size is definitely a stretched widget here
                var isWidthStretched = Mathf.Approximately(width, 0f);
                var isHeightStretched = Mathf.Approximately(height, 0f);

                return new Vector2(
                    x: isWidthStretched ? float.PositiveInfinity : width,
                    y: isHeightStretched ? float.PositiveInfinity : height
                );
            }
        }

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

        // This method provides the bridge TO the old layout system.
        // When a legacy widget contains a new layout-aware widget, this is called.
        public virtual WidgetSize CalculateSize()
        {
            var ro = RenderObject;

            // For legacy parents, report the unconstrained intrinsic size.
            // This is a "best guess" since no constraints are provided.
            var intrinsicWidth = ro.GetIntrinsicWidth(float.PositiveInfinity);
            var intrinsicHeight = ro.GetIntrinsicHeight(intrinsicWidth);

            var isWidthStretched = float.IsInfinity(intrinsicWidth);
            var isHeightStretched = float.IsInfinity(intrinsicHeight);

            return new WidgetSize(
                minWidth: isWidthStretched ? 0 : intrinsicWidth,
                minHeight: isHeightStretched ? 0 : intrinsicHeight,
                maxWidth: isWidthStretched ? float.PositiveInfinity : intrinsicWidth,
                maxHeight: isHeightStretched ? float.PositiveInfinity : intrinsicHeight
            );
        }
    }
}