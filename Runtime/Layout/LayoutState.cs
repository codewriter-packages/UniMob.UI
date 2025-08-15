using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    // Interface for easier type-checking later without generics.
    public interface ILayoutState : IViewState
    {
        RenderObject RenderObject { get; }
    }

    /// <summary>
    /// The base State for a LayoutWidget. It creates and owns the RenderObject.
    /// </summary>
    public abstract class LayoutState<TWidget> : ViewState<TWidget>, ILayoutState where TWidget : LayoutWidget, Widget
    {
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

        internal sealed override void Update(Widget widget)
        {
            base.Update(widget);
            // If the widget is updated, we need to reset the RenderObject.
            this._renderObject = null;
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
    }
}