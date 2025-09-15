using System;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{

    /// <summary>
    /// A generic render object that acts as a proxy for another render object, forwarding layout and rendering
    /// to its child. This is useful for creating wrapper widgets that modify the behavior of their child
    /// without changing its layout logic (e.g. a clickable button that sizes itself to its child).
    /// </summary>
    public class RenderProxy : SingleChildRenderObject
    {
        private readonly ISingleChildLayoutState _state;

        public RenderProxy(ISingleChildLayoutState state) : base(state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            if (Child != null)
            {
                ChildSize = LayoutChild(Child, constraints);
                return ChildSize;
            }
            else
            {
                // If there is NO child, size ourselves to the smallest size
                // the parent's constraints will allow.
                return constraints.Constrain(Vector2.zero);
            }
        }

        protected override void PerformPositioning()
        {
            ChildPosition = Vector2.zero;
        }

    }
}