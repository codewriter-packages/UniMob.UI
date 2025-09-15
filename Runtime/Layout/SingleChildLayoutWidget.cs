#nullable enable
namespace UniMob.UI.Layout
{
    public abstract class SingleChildLayoutWidget : StatefulWidget
    {
        public Widget? Child { get; set; }
    }

    public abstract class SingleChildLayoutState<TWidget> : ViewState<TWidget>, ISingleChildLayoutState
        where TWidget : SingleChildLayoutWidget
    {
        private readonly StateHolder _child;

        public IState Child => _child.Value;

        protected SingleChildLayoutState()
        {
            _child = CreateChild(_ => Widget.Child ?? new Widgets.Empty());
        }
    }
}
