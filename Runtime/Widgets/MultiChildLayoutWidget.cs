using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public abstract class MultiChildLayoutWidget : StatefulWidget
    {
        protected MultiChildLayoutWidget(
            [NotNull] List<Widget> children,
            [CanBeNull] Key key
        ) : base(
            key)
        {
            Children = children ?? throw new ArgumentNullException(nameof(children));
        }

        [NotNull] public List<Widget> Children { get; }
    }
    
    internal abstract class MultiChildLayoutState<TWidget> : ViewState<TWidget>
        where TWidget : MultiChildLayoutWidget
    {
        private readonly Atom<IState[]> _children;

        protected MultiChildLayoutState()
        {
            _children = CreateChildren(context => Widget.Children);
        }

        public IState[] Children => _children.Value;
    }
}