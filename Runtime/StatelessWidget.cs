using System;
using JetBrains.Annotations;
using UniMob.UI.Layout;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI
{
    /// <summary>
    /// A StatelessWidget is a widget that does not own mutable state.
    /// </summary>
    public abstract class StatelessWidget : Widget
    {
        private Type _type;

        public Type Type => _type ?? (_type = GetType());

        [CanBeNull] public Key Key { get; set; }

        public abstract Widget Build(BuildContext context);


        public virtual RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return state.InnerViewState.RenderObject;
        }


        [CanBeNull]
        public virtual State CreateState(StateProvider provider) => null;

        [NotNull]
        public virtual State CreateState()
        {
            return new StatelessElement(this);
        }
    }

    /// <summary>
    /// An internal State object that manages a StatelessWidget's lifecycle.
    /// It acts as a lightweight proxy, holding the immutable widget and calling its Build method.
    /// It delegates its View and Size properties to its child.
    /// </summary>
    internal sealed class StatelessElement : State
    {
        private StateHolder _stateHolder;

        public override IViewState InnerViewState => _stateHolder.Value.InnerViewState;
        public override WidgetSize Size => _stateHolder.Value.Size;

        public StatelessElement(StatelessWidget widget)
        {
            _stateHolder = Create<Widget, IState>(StateLifetime, new BuildContext(this, Context), widget.Build);
        }

        internal override void Update(Widget widget)
        {
            base.Update(widget);

            if (widget is StatelessWidget statelessWidget)
            {
                _stateHolder = Create<Widget, IState>(StateLifetime, new BuildContext(this, Context),
                    statelessWidget.Build);
            }
            else
            {
                throw new InvalidOperationException("StatelessElement can only update with a StatelessWidget");
            }
        }
    }
}