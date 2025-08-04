using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace UniMob.UI
{
    public abstract class StatelessWidget : Widget
    {
        private Type _type;

        public Type Type => _type ?? (_type = GetType());

        [CanBeNull] public Key Key { get; set; }

        public abstract Widget Build(BuildContext context);

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
    internal class StatelessElement : State
    {
        private readonly StateHolder _child;

        public StatelessWidget StatelessWidget { get; private set; }

        // Delegates to the child's state to properly integrate into the view tree.
        public override IViewState InnerViewState => _child.Value.InnerViewState;
        public override WidgetSize Size => _child.Value.Size;

        public StatelessElement(StatelessWidget widget)
        {
            StatelessWidget = widget;
            _child = Create<Widget, IState>(StateLifetime, new BuildContext(this, Context), Build);
        }

        private Widget Build(BuildContext context)
        {
            return StatelessWidget.Build(context);
        }

        internal override void Update(Widget widget)
        {
            base.Update(widget);

            if (widget is StatelessWidget statelessWidget)
            {
                StatelessWidget = statelessWidget;
            }
        }
    }
}
