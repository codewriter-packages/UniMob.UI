using JetBrains.Annotations;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public sealed class Container : SingleChildLayoutWidget
    {
        public Container(
            [CanBeNull] Widget child = null,
            [CanBeNull] Key key = null,
            [CanBeNull] Color? backgroundColor = null,
            [CanBeNull] WidgetSize? size = null,
            [CanBeNull] Alignment? alignment = null)
            : base(child ?? new Empty(), key)
        {
            BackgroundColor = backgroundColor ?? Color.clear;
            Alignment = alignment ?? Alignment.Center;
            Size = size;
        }

        public Color BackgroundColor { get; }
        public Alignment Alignment { get; }
        public WidgetSize? Size { get; }

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