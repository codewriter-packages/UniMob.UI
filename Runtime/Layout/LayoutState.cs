using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout
{
    // Interface for easier type-checking later without generics.
    public interface ILayoutState : IViewState
    {
        RenderObject RenderObject { get; }

        /// <summary>
        ///     <b>[Atom]</b> Performs re-layout on RenderObject if necessary (e.g. constraints or dependencies have changed)
        ///     and subscribes to future re-layouts via the UniMob's reactivity system.
        /// </summary>
        /// <remarks>
        ///     This method can be safely called multiple times because
        ///     it will not cause the layout to be recalculated every time.
        /// </remarks>
        void WatchedPerformLayout();
    }

    /// <summary>
    ///     The base State for a LayoutWidget. It creates and owns the RenderObject.
    /// </summary>
    public abstract class LayoutState<TWidget> : ViewState<TWidget>, ILayoutState where TWidget : LayoutWidget, Widget
    {
        private RenderObject _renderObject;
        private int _renderVersion = int.MinValue;
        private Atom<int> _trackedLayoutPerformer;

        public RenderObject RenderObject
        {
            get
            {
                if (_renderObject == null) _renderObject = Widget.CreateRenderObject(Context, this);
                return _renderObject;
            }
        }

        void ILayoutState.WatchedPerformLayout()
        {
            _trackedLayoutPerformer ??= CreateTrackedLayout();
            _trackedLayoutPerformer.Get();
        }

        // This method provides the bridge TO the old layout system.
        // When a legacy widget contains a new layout-aware widget, this is called.
        public sealed override WidgetSize CalculateSize()
        {
#if UNITY_EDITOR
            if (Context.Parent.State is Widgets.RowState or Widgets.ColumnState)
                Debug.LogWarning("CalculateSize() on a LayoutState is meant as a last resort for interop " +
                                 "with the legacy layout system. \n" +
                                 $"Consider wrapping your modern widgets in a {nameof(LayoutHost)} for " +
                                 "smoother integration.");
#endif
            var ro = RenderObject;

            var intrinsicWidth = ro.GetIntrinsicWidth(float.PositiveInfinity);
            var intrinsicHeight = ro.GetIntrinsicHeight(float.PositiveInfinity);

            var minWidth = float.IsInfinity(intrinsicWidth) ? 0 : intrinsicWidth;
            var maxWidth = float.IsInfinity(intrinsicWidth) ? float.PositiveInfinity : intrinsicWidth;

            var minHeight = float.IsInfinity(intrinsicHeight) ? 0 : intrinsicHeight;
            var maxHeight = float.IsInfinity(intrinsicHeight) ? float.PositiveInfinity : intrinsicHeight;

            return new WidgetSize(minWidth, minHeight, maxWidth, maxHeight);
        }

        private Atom<int> CreateTrackedLayout()
        {
            return Atom.Computed(StateLifetime, () =>
            {
                // PerformLayout() implicitly uses many [Atom] so trackedLayoutPerformer will be auto recomputed.
                RenderObject.PerformLayoutImmediate(Constraints);

                // Also recompute layout on Widget and Constraints modifications.
                _ = Widget;
                _ = Constraints;

                // The PerformLayout() was done and we need all subscribers to be invalidated,
                // so we always return a new number.
                return _renderVersion = (_renderVersion + 1) % int.MaxValue;
            });
        }
    }
}