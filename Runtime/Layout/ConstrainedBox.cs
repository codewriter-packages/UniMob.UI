using System;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    public class ConstrainedBox : LayoutWidget
    {
        public Widget Child { get; set; }
        public LayoutConstraints BoxConstraints { get; set; }

        public override State CreateState()
        {
            return new ConstrainedBoxState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderConstrainedBox((ConstrainedBoxState) state);
        }
    }

    public interface IConstrainedBoxState : ISingleChildLayoutState
    {
        IState Child { get; }
        LayoutConstraints BoxConstraints { get; }
    }

    public class ConstrainedBoxState : LayoutState<ConstrainedBox>, IConstrainedBoxState
    {
        private readonly StateHolder _child;
        public IState Child => _child.Value;
        public LayoutConstraints BoxConstraints => Widget.BoxConstraints;


        public ConstrainedBoxState()
        {
            _child = CreateChild(c => Widget.Child);
        }


        public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.ConstrainedBoxView");
    }
}