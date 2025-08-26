using JetBrains.Annotations;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// Represents a widget that constrains its child's size to a specific width and/or height.
    /// </summary>
    public class SizedBox : StatefulWidget
    {
        public Widget? Child { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }

        public SizedBox(float? width = null, float? height = null)
        {
            this.Width = width;
            this.Height = height;
        }

        // Factory methods
        public override State CreateState()
        {
            return new SizedBoxState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return new RenderSizedBox((ISizedBoxState) state);
        }

        public static SizedBox FromWidth(float width, Widget? child = null)
        {
            return new SizedBox(width, null) {Child = child};
        }

        public static SizedBox FromHeight(float height, Widget? child = null)
        {
            return new SizedBox(null, height);
        }

        public static SizedBox Square(float size, Widget? child = null)
        {
            return new SizedBox(size, size) {Child = child};
        }

        public static SizedBox Shrink()
        {
            return new SizedBox(0, 0);
        }
        
        public static SizedBox Expand(Widget? child = null)
        {
            return new SizedBox(float.PositiveInfinity, float.PositiveInfinity) {Child = child};
        }
    }

    public interface ISizedBoxState : ISingleChildLayoutState
    {
        float? Width { get; }
        float? Height { get; }
        Alignment Alignment { get; }
    }

    public class SizedBoxState : ViewState<SizedBox>, ISizedBoxState
    {
        public IState Child => _child.Value;
        private readonly StateHolder _child;

        public SizedBoxState()
        {
            _child = CreateChild(context => Widget.Child ?? new Widgets.Empty());
        }

        public float? Width => Widget.Width;
        public float? Height => Widget.Height;
        public Alignment Alignment => Alignment.Center; // Default alignment for SizedBox


        public override WidgetViewReference View =>
            WidgetViewReference.Resource("$$_Layout.SizedBoxView");
    }
}