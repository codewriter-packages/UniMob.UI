#nullable enable
using System;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderAlign : RenderObject, ISingleChildRenderObject
    {
        private readonly AlignState _state;

        public Vector2 ChildPosition { get; private set; }
        public Vector2 ChildSize { get; private set; }

        public RenderAlign(AlignState state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var child = _state.Child;

            // 1. Give the child LOOSE constraints to discover its ideal, shrink-wrapped size.
            var childConstraints = new LayoutConstraints(0, 0, float.PositiveInfinity, float.PositiveInfinity);
            ChildSize = LayoutChild(child, childConstraints);

            // 2. The Align widget's own size BECOMES the child's size, but clamped by the parent's rules.
            return new Vector2(
                Mathf.Clamp(ChildSize.x, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(ChildSize.y, constraints.MinHeight, constraints.MaxHeight)
            );
        }

        protected override void PerformPositioning()
        {
            // The Align widget's job is done after sizing. Its child is positioned
            // at (0,0) relative to the Align widget itself. The parent of the Align
            // widget is responsible for aligning it.
            this.ChildPosition = Vector2.zero;
        }

        public override float GetIntrinsicWidth(float height)
        {
            return _state.Child.RenderObject.GetIntrinsicWidth(height);
        }

        public override float GetIntrinsicHeight(float width)
        {
            return _state.Child.RenderObject.GetIntrinsicHeight(width);
        }
    }
}