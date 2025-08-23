// In a new file: Runtime/Layout/Internal/RenderObjects/RenderPadding.cs

using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderPadding : RenderObject, ISingleChildRenderObject
    {
        private readonly IPaddingState _state;

        public Vector2 ChildPosition { get; private set; }
        public Vector2 ChildSize { get; private set; }

        public RenderPadding(IPaddingState state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var padding = _state.Padding;
            
            // 1. Deflate the parent's constraints by the padding amount.
            // This creates the smaller box in which the child can be laid out.
            var innerConstraints = constraints.Deflate(padding);
            
            // 2. Lay out the child within those smaller, inner constraints.
            ChildSize = LayoutChild(_state.Child, innerConstraints);
            
            // 3. The final size of the Padding widget is the child's size
            // plus the padding on all sides.
            var finalWidth = ChildSize.x + padding.Horizontal;
            var finalHeight = ChildSize.y + padding.Vertical;
            
            // Ensure the final size still respects the original parent constraints.
            return constraints.Constrain(new Vector2(finalWidth, finalHeight));
        }

        protected override void PerformPositioning()
        {
            var padding = _state.Padding;
            
            // The child is simply positioned at an offset equal to the top-left padding.
            ChildPosition = new Vector2(padding.Left, padding.Top);
        }

        public override float GetIntrinsicWidth(float height)
        {
            var padding = _state.Padding;
            var childIntrinsicWidth = GetChildIntrinsicWidth(_state.Child, height - padding.Vertical);
            return childIntrinsicWidth + padding.Horizontal;
        }

        public override float GetIntrinsicHeight(float width)
        {
            var padding = _state.Padding;
            var childIntrinsicHeight = GetChildIntrinsicHeight(_state.Child, width - padding.Horizontal);
            return childIntrinsicHeight + padding.Vertical;
        }
        
        // Helper methods to correctly query child's intrinsic size
        private float GetChildIntrinsicWidth(IState child, float height)
        {
            return child.RenderObject.GetIntrinsicWidth(height);
        }

        private float GetChildIntrinsicHeight(IState child, float width)
        {
            return child.RenderObject.GetIntrinsicHeight(width);
        }
    }
}