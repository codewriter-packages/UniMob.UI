using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;

namespace UniMob.UI.Layout
{
    // Interface for easier type-checking later without generics.
    public interface ILayoutState : IViewState
    {
        RenderObject RenderObject { get; }

        /// <summary>
        /// <b>[Atom]</b> Performs re-layout on RenderObject if necessary (e.g. constraints or dependencies have changed)
        /// and subscribes to future re-layouts via the UniMob's reactivity system.
        /// </summary>
        /// <remarks>
        /// This method can be safely called multiple times because
        /// it will not cause the layout to be recalculated every time.
        /// </remarks>
        /// <returns>Final render size of the widget.</returns>
        Vector2 WatchedPerformLayout();
    }

    /// <summary>
    /// The base State for a LayoutWidget. It creates and owns the RenderObject.
    /// </summary>
    public abstract class LayoutState<TWidget> : ViewState<TWidget>, ILayoutState where TWidget : LayoutWidget, Widget
    {
        private int _renderVersion = int.MinValue;
        private Atom<(Vector2 renderSize, int version)> _trackedLayoutPerformer;

        private RenderObject _renderObject;

        public RenderObject RenderObject
        {
            get
            {
                if (_renderObject == null)
                {
                    _renderObject = Widget.CreateRenderObject(Context, this);
                }

                return _renderObject;
            }
        }

        // This method provides the bridge TO the old layout system.
        // When a legacy widget contains a new layout-aware widget, this is called.
        public override WidgetSize CalculateSize()
        {
            var ro = RenderObject;

            // For legacy parents, report the unconstrained intrinsic size.
            // This is a "best guess" since no constraints are provided.
            var intrinsicWidth = ro.GetIntrinsicWidth(float.PositiveInfinity);
            var intrinsicHeight = ro.GetIntrinsicHeight(intrinsicWidth);

            return WidgetSize.Fixed(intrinsicWidth, intrinsicHeight);
        }

        Vector2 ILayoutState.WatchedPerformLayout()
        {
            _trackedLayoutPerformer ??= CreateTrackedLayout();

            var renderData = _trackedLayoutPerformer.Get();

            return renderData.renderSize;
        }

        private Atom<(Vector2 renderSize, int version)> CreateTrackedLayout()
        {
            return Atom.Computed(StateLifetime, () =>
            {
                // PerformLayout() implicitly uses many [Atom] so trackedLayoutPerformer will be auto recomputed.
                RenderObject.PerformLayoutImmediate(Constraints);

                // Also recompute layout on Widget and Constraints modifications.
                _ = Widget;
                _ = Constraints;

                var renderSize = RenderObject.Size;

                // The PerformLayout() was done and we need all subscribers to be invalidated,
                // so we always return a new number.
                return (renderSize, _renderVersion = ((_renderVersion + 1) % int.MaxValue));
            });
        }
    }
}