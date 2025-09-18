using JetBrains.Annotations;
using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// A widget that paints a given color or image onto its area.
    /// It does not change the layout of its child, if any.
    /// </summary>
    public class ColoredImageBox : SingleChildLayoutWidget
    {
        public Color Color { get; set; } = Color.clear;
        
        [CanBeNull] public Sprite Image { get; set; } = null;

        public override State CreateState() => new ColoredImageBoxState();

        // This widget does not affect the layout of its child.
        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return new RenderProxy((ColoredImageBoxState) state);
        }
    }

    internal interface IColoredImageBoxState : ISingleChildLayoutState
    {
        Color BackgroundColor { get; }
        Sprite BackgroundImage { get; }
    }

    internal class ColoredImageBoxState : SingleChildLayoutState<ColoredImageBox>, IColoredImageBoxState
    {
        public Color BackgroundColor => Widget.Color;

        public Sprite BackgroundImage => Widget.Image != null ? Widget.Image : UniMobViewContext.DefaultWhiteImage;

        public override WidgetViewReference View =>
            WidgetViewReference.Resource("$$_Layout.ColoredImageBoxView");
    }
}