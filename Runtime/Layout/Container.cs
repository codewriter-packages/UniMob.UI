using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout
{
    public class Container : LayoutWidget
    {
        public Widget Child { get; set; }
        public Color BackgroundColor { get; set; } = Color.clear;
        public Sprite BackgroundImage { get; set; } = null; // Or implement as needed
        public Alignment Alignment { get; set; } = Alignment.Center;
        public float? Width { get; set; }
        public float? Height { get; set; }

        public override State CreateState()
        {
            return new ContainerState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderContainer((ContainerState) state);
        }
    }

    // A shared interface for container-like states
    internal interface IContainerState : ISingleChildLayoutState
    {
        Color BackgroundColor { get; }
        Sprite BackgroundImage { get; }
    }

    public class ContainerState : LayoutState<Container>, IContainerState
    {
        private readonly StateHolder _child;

        public ContainerState()
        {
            _child = CreateChild(context => Widget.Child ?? new Empty());
        }

        public Alignment Alignment => Widget.Alignment;

        public IState Child => _child.Value;
        public Color BackgroundColor => Widget.BackgroundColor;

        public Sprite BackgroundImage => Widget.BackgroundImage != null
            ? Widget.BackgroundImage
            : UniMobViewContext.DefaultWhiteImage;

        public override WidgetViewReference View =>
            WidgetViewReference.Resource("$$_Layout.ContainerView");
    }
}