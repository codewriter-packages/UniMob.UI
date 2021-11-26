using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class HocState<TWidget> : State
        where TWidget : Widget
    {
        private readonly StateHolder _child;

        protected TWidget Widget { get; private set; }

        public sealed override WidgetSize Size => InnerViewState.Size;

        public sealed override IViewState InnerViewState => _child.Value.InnerViewState;

        protected HocState()
        {
            _child = Create<Widget, IState>(StateLifetime, new BuildContext(this, Context), Build);
        }

        internal sealed override void Update(Widget widget)
        {
            base.Update(widget);

            var oldWidget = Widget;

            if (widget is TWidget typedWidget)
            {
                Widget = typedWidget;
            }
            else
            {
                throw new WrongStateTypeException(GetType(), typeof(TWidget), widget.GetType());
            }

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