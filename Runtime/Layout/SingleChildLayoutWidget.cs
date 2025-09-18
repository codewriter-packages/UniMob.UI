#nullable enable
namespace UniMob.UI.Layout
{
    /// <summary>
    /// Represents a widget that manages the layout of a single child widget.
    /// </summary>
    public abstract class SingleChildLayoutWidget : StatefulWidget
    {
        public Widget? Child { get; set; }
    }

    /// <summary>
    /// Represents the state of a layout that manages a single child widget.
    /// </summary>
    /// <remarks>
    /// This class provides a default implementation for the <see cref="View"/> property, which returns a reference to a
    /// non-painting layout view. Subclasses can override this property if they need to provide a different view.
    /// </remarks>
    public abstract class SingleChildLayoutState<TWidget> : ViewState<TWidget>, ISingleChildLayoutState
        where TWidget : SingleChildLayoutWidget
    {
        private readonly StateHolder _child;

        public IState Child => _child.Value;

        protected SingleChildLayoutState()
        {
            _child = CreateChild(_ => Widget.Child ?? new Widgets.Empty());
        }

        public override WidgetViewReference View =>
            WidgetViewReference.Resource("$$_Layout.SingleChildLayoutView");
    }
}
