#nullable enable
using JetBrains.Annotations;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// A container widget that can hold a single child widget and provides layout and painting options.
    /// </summary>
    public class Container : StatefulWidget
    {
        [CanBeNull]
        public Widget? Child { get; set; }
        public Color BackgroundColor { get; set; } = Color.clear;
        public Sprite? BackgroundImage { get; set; } = null;
        public Alignment? Alignment { get; set; } = null;
        public float? Width { get; set; }
        public float? Height { get; set; }

        public Container(float? width = null, float? height = null)
        {
            Width = width;
            Height = height;
        }

        public override State CreateState()
        {
            return new ContainerState();
        }
    }

    public class ContainerState : HocState<Container>
    {
        public override Widget Build(BuildContext context)
        {
            var current = Widget.Child;

            if (Widget.Alignment is Alignment alignment)
            {
                current = new Align { Alignment = alignment, Child = current };
            }

            // Wrap with a painting box if color/image is set.
            if (Widget.BackgroundColor != Color.clear || Widget.BackgroundImage != null)
            {
                current = new ColoredImageBox
                {
                    Color = Widget.BackgroundColor,
                    Image = Widget.BackgroundImage,
                    Child = current,
                };
            }

            // Wrap with a sizing widget if width/height is set.
            if (Widget.Width != null || Widget.Height != null)
            {
                current = new SizedBox(child: current, width: Widget.Width, height: Widget.Height);
            }

            return current ?? new Empty();
        }
    }
}
