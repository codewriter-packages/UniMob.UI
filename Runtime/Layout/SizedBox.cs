using JetBrains.Annotations;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// Represents a widget that constrains its child's size to a specific width and/or height.
    /// </summary>
    public class SizedBox : LayoutWidget
    {
        [CanBeNull] public Widget Child { get; set; }
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
        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderContainer((ISizedBoxState) state);
        }
        
        public static SizedBox FromWidth(float width)
        {
            return new SizedBox(width, null);
        }
        public static SizedBox FromHeight(float height)
        {
            return new SizedBox(null, height);
        }
        public static SizedBox Square(float size)
        {
            return new SizedBox(size, size);
        }
        public static SizedBox Shrink()
        {
            return new SizedBox(0, 0);
        }
        public static SizedBox Expand()
        {
            return new SizedBox(float.PositiveInfinity, float.PositiveInfinity);
        }
    }
    
    internal interface ISizedBoxState : ISingleChildLayoutState
    {
        float? Width { get; }
        float? Height { get; }
        Alignment Alignment { get; }
    }
    
    internal class SizedBoxState : LayoutState<SizedBox>, ISizedBoxState
    {
        public IState Child => _child.Value;
        private readonly StateHolder _child;

        public SizedBoxState()
        {
            _child = CreateChild(context => Widget.Child ?? SizedBox.Shrink());
        }

        public float? Width => Widget.Width;
        public float? Height => Widget.Height;
        public Alignment Alignment => Alignment.Center; // Default alignment for SizedBox


        public override WidgetViewReference View =>
            WidgetViewReference.Resource("$$_Layout.SingleChildLayoutView");
    }
}