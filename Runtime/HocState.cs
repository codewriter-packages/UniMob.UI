using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public interface IHocState : IState
    {
    }

    public abstract class HocState<TWidget> : State, IHocState
        where TWidget : Widget
    {
        private readonly StateHolder _child;
        private readonly MutableAtom<TWidget> _widget = Atom.Value(default(TWidget));

        protected TWidget Widget => _widget.Value;

        public sealed override WidgetSize Size => InnerViewState.Size;

        public sealed override IViewState InnerViewState => _child.Value.InnerViewState;

        protected HocState()
        {
            _child = Create<Widget, IState>(StateLifetime, new BuildContext(this, Context), Build);
        }

        internal sealed override void Update(Widget widget)
        {
            if (widget is not TWidget typedWidget)
            {
                throw new WrongStateTypeException(GetType(), typeof(TWidget), widget.GetType());
            }

            var oldWidget = Widget;

            _widget.Value = typedWidget;

            base.Update(widget);

            if (oldWidget != null)
            {
                DidUpdateWidget(oldWidget);
            }
        }

        public abstract Widget Build(BuildContext context);

        public virtual void DidUpdateWidget([NotNull] TWidget oldWidget)
        {
            Assert.IsNull(Atom.CurrentScope);
        }
    }
}