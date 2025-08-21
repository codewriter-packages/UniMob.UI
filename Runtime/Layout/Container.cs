using JetBrains.Annotations;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout
{
    internal interface IContainerState : ISizedBoxState
    {
        Color BackgroundColor { get; }
        Sprite BackgroundImage { get; }
    }

    
    /// <summary>
    /// A container widget that can hold a single child widget and provides layout options such as alignment,
    /// background color, and size.
    /// </summary>
    public class Container : LayoutWidget
    {
        [CanBeNull] public Widget Child { get; set; }
        public Color BackgroundColor { get; set; } = Color.clear;
        public Sprite BackgroundImage { get; set; } = null;
        public Alignment Alignment { get; set; } = Alignment.Center;
        public float? Width { get; set; }
        public float? Height { get; set; }

        public Container(float? width = null, float? height = null)
        {
            this.Width = width;
            this.Height = height;
        }

        public override State CreateState()
        {
            return new ContainerState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderContainer((ISizedBoxState) state);
        }
    }


    internal class ContainerState : LayoutState<Container>, IContainerState
    {
        private readonly StateHolder _child;

        public ContainerState()
        {
            _child = CreateChild(context => Widget.Child ?? new Empty());
        }

        public Alignment Alignment => Widget.Alignment;

        public float? Width => Widget.Width;
        public float? Height => Widget.Height;

        public IState Child => _child.Value;
        public Color BackgroundColor => Widget.BackgroundColor;

        public Sprite BackgroundImage => Widget.BackgroundImage != null
            ? Widget.BackgroundImage
            : UniMobViewContext.DefaultWhiteImage;

        public override WidgetViewReference View =>
            WidgetViewReference.Resource("$$_Layout.ContainerView");
    }
}