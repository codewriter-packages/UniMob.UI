using System;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    
    /// <summary>
    /// A generic render object that acts as a proxy for another render object, forwarding layout and rendering
    /// to its child. This is useful for creating wrapper widgets that modify the behavior of their child
    /// without changing its layout logic (e.g. a clickable button that sizes itself to its child).
    /// </summary>
    public class RenderProxy : RenderObject, ISingleChildRenderObject
    {
        private readonly ISingleChildLayoutState _state;

        public Vector2 ChildSize { get; private set; }
        public Vector2 ChildPosition { get; private set; }

        public RenderProxy(ISingleChildLayoutState state)
        {
            _state = state;
        }
        
        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            // We pass the parent's constraints directly to the child, transparently forwarding them.
            ChildSize = LayoutChild(_state.Child, constraints);
            
            // The proxy's size is exactly the size of our child.
            return ChildSize;
        }

        protected override void PerformPositioning()
        {
            // Since this is a simple proxy, we position the child at the origin (0,0).
            ChildPosition = Vector2.zero;
        }

        public override float GetIntrinsicWidth(float height)
        {
            if (_state.Child.AsLayoutState(out var cls)) return cls.RenderObject.GetIntrinsicWidth(height);
            return _state.Child?.Size.GetSizeUnbounded().x ?? 0;
        }

        public override float GetIntrinsicHeight(float width)
        {
            if (_state.Child.AsLayoutState(out var cls)) return cls.RenderObject.GetIntrinsicHeight(width);
            return _state.Child?.Size.GetSizeUnbounded().y ?? 0;
        }

    }
}