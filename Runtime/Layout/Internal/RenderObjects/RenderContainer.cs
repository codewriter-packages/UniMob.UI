using System;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderContainer : RenderObject, ISingleChildRenderObject
    {
        private readonly ISizedBoxState _state;

        public RenderContainer(ISizedBoxState state)
        {
            _state = state;
        }

        private Container Widget => (Container) _state.RawWidget;

        // ADD THIS: A place to store the final result of the positioning pass.
        public Vector2 ChildPosition { get; private set; }
        public Vector2 ChildSize { get; private set; }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var widget = Widget;
            
            // These are the constraints that the Container widget imposes on itself.
            // They are equivalent to an unbounded constraints if the widget's Width or Height are not set.
            var selfConstraints = LayoutConstraints.Loose(
                widget.Width ?? float.PositiveInfinity, 
                widget.Height ?? float.PositiveInfinity
            );
            
            // The child's constraints are the parent's constraints, further restricted
            // by this container's own rules. We enforce our bounds and then loosen the
            // result so the child can choose any size up to the determined maximum.
            var childConstraints = constraints.Enforce(selfConstraints).Loosen();
            
            // Measure the child with the loosened constraints to discover its ideal size.
            ChildSize = LayoutChild(_state.Child, childConstraints);

            
            // Now we determine the final size of this Container widget, by applying the
            // Width and Height properties if they are set, or using the child's size otherwise.
            var finalWidth = widget.Width ??
                             (float.IsInfinity(constraints.MaxWidth) ? ChildSize.x : constraints.MaxWidth);
            var finalHeight = widget.Height ??
                              (float.IsInfinity(constraints.MaxHeight) ? ChildSize.y : constraints.MaxHeight);
            
            // Constrain the final size to the parent's constraints.
            return constraints.Constrain(new Vector2(finalWidth, finalHeight));
        }

        protected override void PerformPositioning()
        {
            var child = _state.Child;
            var alignment = _state.Alignment;
            if (child == null)
            {
                ChildPosition = Vector2.zero;
                return;
            }


            var x = (Size.x - ChildSize.x) * (alignment.X * 0.5f + 0.5f);
            var y = (Size.y - ChildSize.y) * (alignment.Y * 0.5f + 0.5f);

            ChildPosition = new Vector2(x, y);
        }

        public override float GetIntrinsicWidth(float height)
        {
            var widget = Widget;

            if (widget.Width.HasValue) return widget.Width.Value;

            if (_state.Child.AsLayoutState(out var childLayoutState))
                return childLayoutState.RenderObject.GetIntrinsicWidth(height);

            // Fallback for legacy widgets
            return _state.Child?.Size.GetSizeUnbounded().x ?? 0;
        }

        public override float GetIntrinsicHeight(float width)
        {
            var widget = Widget;

            if (widget.Height.HasValue) return widget.Height.Value;

            if (_state.Child.AsLayoutState(out var childLayoutState))
                return childLayoutState.RenderObject.GetIntrinsicHeight(width);

            // Fallback for legacy widgets
            return _state.Child?.Size.GetSizeUnbounded().y ?? 0;
        }
    }
}