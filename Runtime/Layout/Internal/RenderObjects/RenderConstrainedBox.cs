using System;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderConstrainedBox : RenderObject, ISingleChildRenderObject
    {
        private readonly IConstrainedBoxState _state;
        public Vector2 ChildSize { get; private set; }
        public Vector2 ChildPosition { get; private set; }

        public RenderConstrainedBox(IConstrainedBoxState state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var selfConstraints = _state.BoxConstraints;

            // 2. Combine the parent's constraints with the widget's own constraints,
            //    taking the most restrictive rules for both min and max.
            var childConstraints = constraints.Enforce(selfConstraints);

            // 3. Lay out the child with these final, combined constraints.
            ChildSize = LayoutChild(_state.Child, childConstraints);

            // A ConstrainedBox sizes itself to its child.
            return ChildSize;
        }

        protected override void PerformPositioning()
        {
            ChildPosition = Vector2.zero;
        }

        // Intrinsic sizing is also delegated directly to the child, but constrained
        // by the BoxConstraints.
        public override float GetIntrinsicWidth(float height)
        {
            var childIntrinsic = GetChildIntrinsicWidth(_state.Child, height);
            return Mathf.Clamp(childIntrinsic,_state.BoxConstraints.MinWidth, _state.BoxConstraints.MaxWidth);
        }

        public override float GetIntrinsicHeight(float width)
        {
            
            var childIntrinsic = GetChildIntrinsicHeight(_state.Child, width);
            return Mathf.Clamp(childIntrinsic, _state.BoxConstraints.MinHeight, _state.BoxConstraints.MaxHeight);
        }
        
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