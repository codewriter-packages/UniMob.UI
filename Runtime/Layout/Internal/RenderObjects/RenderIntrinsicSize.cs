using System;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    public class RenderIntrinsicSize : RenderObject
    {
        private readonly IntrinsicSizeState _state;

        private IntrinsicSize Widget => (IntrinsicSize) _state.RawWidget;

        public RenderIntrinsicSize(IntrinsicSizeState state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var widget = this.Widget;

            var size = ComputeIntrinsicSize(widget.Axis);

            var childConstraints = LayoutConstraints.Tight(
                Mathf.Clamp(size.x, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(size.y, constraints.MinHeight, constraints.MaxHeight)
            );

            var childSize = LayoutChild(_state.Child, childConstraints);

            return new Vector2(
                Mathf.Clamp(childSize.x, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(childSize.y, constraints.MinHeight, constraints.MaxHeight)
            );
        }

        protected override void PerformPositioning()
        {
        }

        private Vector2 ComputeIntrinsicSize(Axis axis)
        {
            if (axis == Axis.Horizontal)
            {
                var width = GetIntrinsicWidth(float.PositiveInfinity);
                var height = GetIntrinsicHeight(width);
                return new Vector2(width, height);
            }
            else
            {
                var height = GetIntrinsicHeight(float.PositiveInfinity);
                var width = GetIntrinsicWidth(height);
                return new Vector2(width, height);
            }
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