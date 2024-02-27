using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public abstract class MultiChildLayoutWidget : StatefulWidget
    {
        [NotNull] public List<Widget> Children { get; set; } = new List<Widget>();
        [CanBeNull] public Func<List<Widget>> ChildrenBuilder { get; set; }
    }

    public abstract class MultiChildLayoutState<TWidget> : ViewState<TWidget>
        where TWidget : MultiChildLayoutWidget
    {
        private readonly StateCollectionHolder _children;

        protected MultiChildLayoutState()
        {
            _children = CreateChildren(context => Widget.ChildrenBuilder?.Invoke() ?? Widget.Children);
        }

        public IState[] Children => _children.Value;
    }
}