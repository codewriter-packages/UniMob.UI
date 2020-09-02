using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class ViewState<TWidget> : ViewState
        where TWidget : Widget
    {
        private readonly MutableAtom<TWidget> _widget = Atom.Value("widget", default(TWidget));

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

        protected Atom<IState> CreateChild(WidgetBuilder builder)
            => Create(new BuildContext(null, Context), builder);

        protected Atom<IState[]> CreateChildren(Func<BuildContext, List<Widget>> builder)
            => CreateList(new BuildContext(null, Context), builder);
    }
}