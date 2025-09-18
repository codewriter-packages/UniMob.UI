using JetBrains.Annotations;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// Represents a widget that constrains its child's size to a specific width and/or height.
    /// </summary>
    public class SizedBox : SingleChildLayoutWidget
    {
        public float? Width { get; set; }
        public float? Height { get; set; }

        public SizedBox()
        {
        }

        public SizedBox([CanBeNull] Widget child, float? width = null, float? height = null)
        {
            Width = width;
            Height = height;
            Child = child;
        }

        // Factory methods
        public override State CreateState()
        {
            return new SizedBoxState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return new RenderConstrainedBox((IConstrainedBoxState) state);
        }

        public static SizedBox FromWidth(float width, [CanBeNull] Widget child = null)
        {
            return new SizedBox(child, width, null);
        }

        public static SizedBox FromHeight(float height, [CanBeNull] Widget child = null)
        {
            return new SizedBox(child, null, height);
        }

        public static SizedBox Square(float size, [CanBeNull] Widget child = null)
        {
            return new SizedBox(child, size, size);
        }

        public static SizedBox Shrink()
        {
            return new SizedBox(null, 0, 0);
        }

        public static SizedBox Expand([CanBeNull] Widget child = null)
        {
            return new SizedBox(child, float.PositiveInfinity, float.PositiveInfinity);
        }
    }

    public class SizedBoxState : SingleChildLayoutState<SizedBox>, IConstrainedBoxState
    {
        public float? Width => Widget.Width;
        public float? Height => Widget.Height;
        public LayoutConstraints BoxConstraints => LayoutConstraints.TightFor(Width, Height);
    }
}