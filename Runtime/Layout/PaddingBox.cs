// In a new file: Runtime/Layout/Padding.cs

using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Widgets;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// A widget that insets its child by the given padding.
    /// </summary>
    public class PaddingBox : LayoutWidget
    {
        public Widget Child { get; set; }
        public RectPadding Padding { get; set; }

        public PaddingBox(RectPadding padding)
        {
            this.Padding = padding;
        }
        
        public override State CreateState() => new PaddingBoxState();
        
        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderPadding((IPaddingState) state);
        }
    }

    
    internal interface IPaddingState : ISingleChildLayoutState
    {
        RectPadding Padding { get; }
    }

    public class PaddingBoxState : LayoutState<PaddingBox>, IPaddingState
    {
        private readonly StateHolder _child;

        public PaddingBoxState()
        {
            _child = CreateChild(context => Widget.Child);
        }
        
        public IState Child => _child.Value;
        public RectPadding Padding => Widget.Padding;

        // A Padding widget uses a simple view that just hosts the child.
        // The actual padding is applied by the RenderObject during layout.
        public override WidgetViewReference View =>
            WidgetViewReference.Resource("$$_Layout.PaddingBoxView");
    }
}