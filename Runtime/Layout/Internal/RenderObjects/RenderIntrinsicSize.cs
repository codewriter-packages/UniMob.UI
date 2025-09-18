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

            var childConstraints = widget.Axis == Axis.Horizontal
                ? constraints.Tighten(width: GetIntrinsicWidth(float.PositiveInfinity))
                : constraints.Tighten(height: GetIntrinsicHeight(float.PositiveInfinity));

            return LayoutChild(_state.Child, childConstraints);
        }

        protected override void PerformPositioning()
        {
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