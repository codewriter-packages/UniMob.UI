using JetBrains.Annotations;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public sealed class Container : SingleChildLayoutWidget
    {
        public Color BackgroundColor { get; set; } = Color.clear;
        public Alignment Alignment { get; set; } = Alignment.Center;
        public WidgetSize? Size { get; set; }

        public override State CreateState() => new ContainerState();
    }

    internal sealed class ContainerState : SingleChildLayoutState<Container>, IContainerState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Container");

        public Color BackgroundColor => Widget.BackgroundColor;

        public Alignment Alignment => Widget.Alignment;

        public override WidgetSize CalculateSize() => Widget.Size ?? base.CalculateSize();
    }
}