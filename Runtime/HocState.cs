using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class HocState<TWidget> : State
        where TWidget : Widget
    {
        private readonly MutableAtom<TWidget> _widget = Atom.Value("widget", default(TWidget));
        private readonly Atom<IState> _child;

        protected TWidget Widget => _widget.Value;

        public sealed override WidgetSize Size => InnerViewState.Size;

        public sealed override IViewState InnerViewState => _child.Value.InnerViewState;

        protected HocState()
        {
            _child = Create(new BuildContext(null, Context), Build);
        }

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

        public abstract Widget Build(BuildContext context);

        public virtual void DidUpdateWidget([NotNull] TWidget oldWidget)
        {
            Assert.IsNull(Atom.CurrentScope);
        }
    }
}