using System;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public abstract class SingleChildLayoutWidget : StatefulWidget
    {
        [NotNull] public Widget Child { get; set; } = new Empty();
    }

    internal abstract class SingleChildLayoutState<TWidget> : ViewState<TWidget>
        where TWidget : SingleChildLayoutWidget
    {
        private readonly StateHolder _child;

        protected SingleChildLayoutState()
        {
            _child = CreateChild(context => Widget.Child);
        }

        public IState Child => _child.Value;

        public override WidgetSize CalculateSize() => Child.Size;
    }
}