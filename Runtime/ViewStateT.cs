using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class ViewState<TWidget> : ViewState
        where TWidget : Widget
    {
        private readonly MutableAtom<TWidget> _widget = Atom.Value(default(TWidget));

        protected TWidget Widget => _widget.Value;

        internal sealed override void Update(Widget widget)
        {
            base.Update(widget);

            var oldWidget = Widget;

            if (widget is TWidget typedWidget)
            {
                _widget.Value = typedWidget;
            }
            else
            {
                throw new Exception($"Trying to pass {widget.GetType()}, but expected {typeof(TWidget)}");
            }

            if (oldWidget != null)
            {
                DidUpdateWidget(oldWidget);
            }
        }

        public virtual void DidUpdateWidget([NotNull] TWidget oldWidget)
        {
            Assert.IsNull(Atom.CurrentScope);
        }

        protected StateHolder<TChildState> CreateChild<TChildWidget, TChildState>(WidgetBuilder<TChildWidget> builder)
            where TChildWidget : Widget
            where TChildState : ViewState<TChildWidget>
        {
            return Create<TChildWidget, TChildState>(new BuildContext(this, Context), builder);
        }

        protected StateHolder CreateChild(WidgetBuilder<Widget> builder)
        {
            return Create<Widget, IState>(new BuildContext(this, Context), builder);
        }

        protected StateCollectionHolder CreateChildren(Func<BuildContext, List<Widget>> builder)
        {
            return CreateList(new BuildContext(this, Context), builder);
        }
    }
}