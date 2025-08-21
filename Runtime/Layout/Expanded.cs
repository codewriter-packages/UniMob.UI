using System;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    /// <summary>
    ///     A widget that expands a child of a Row or Column to fill the available space.
    ///     This is a signal widget and does not create its own RenderObject.
    /// </summary>
    public class Expanded : LayoutWidget
    {
        public int Flex { get; set; } = 1;
        public Widget Child { get; set; }

        public override State CreateState()
        {
            return new ExpandedState();
        }

        /// <summary>
        ///     Expanded is a proxy. The actual RenderObject is created by its child.
        ///     The parent RenderFlex will look for the Expanded widget on the state.
        /// </summary>
        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            var childState = ((ExpandedState) state).Child;
            if (childState.AsLayoutState(out var layoutChild)) return layoutChild.RenderObject;

            throw new InvalidOperationException("Expanded can only be used with layout-aware widgets.");
        }
    }

    public class ExpandedState : LayoutState<Expanded>
    {
        private readonly StateHolder _child;

        public ExpandedState()
        {
            _child = CreateChild(context => Widget.Child);
        }

        public IState Child => _child.Value;

        public override WidgetViewReference View => Child.InnerViewState.View;
    }
}