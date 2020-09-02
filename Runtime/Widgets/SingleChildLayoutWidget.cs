using System;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public abstract class SingleChildLayoutWidget : StatefulWidget
    {
        protected SingleChildLayoutWidget(
            [NotNull] Widget child,
            [CanBeNull] Key key
        ) : base(key)
        {
            Child = child ?? throw new ArgumentNullException(nameof(child));
        }

        [NotNull] public Widget Child { get; }
    }

    internal abstract class SingleChildLayoutState<TWidget> : ViewState<TWidget>
        where TWidget : SingleChildLayoutWidget
    {
        private readonly Atom<IState> _child;

        protected SingleChildLayoutState()
        {
            _child = CreateChild(context => Widget.Child);
        }

        public IState Child => _child.Value;

        public override WidgetSize CalculateSize() => Child.Size;
    }
}