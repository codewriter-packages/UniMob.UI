using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderContainer : RenderObject, ISingleChildRenderObject
    {
        private readonly ContainerState _state;

        public RenderContainer(ContainerState state)
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

            var childContraints = new LayoutConstraints(
                0, // A child can always choose to be smaller.
                0,
                widget.Width ?? constraints.MaxWidth, // Use own width if set, otherwise parent's.
                widget.Height ?? constraints.MaxHeight // Use own height if set, otherwise parent's.
            );

            // 2. The child's size is now calculated cleanly using the helper,
            //    but with the CORRECT, TIGHTENED constraints.
            ChildSize = LayoutChild(_state.Child, childContraints);

            // 2. SIZING PASS: Determine this container's own size.
            var finalWidth = widget.Width ??
                             (float.IsInfinity(constraints.MaxWidth) ? ChildSize.x : constraints.MaxWidth);
            var finalHeight = widget.Height ??
                              (float.IsInfinity(constraints.MaxHeight) ? ChildSize.y : constraints.MaxHeight);

            return new Vector2(
                Mathf.Clamp(finalWidth, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(finalHeight, constraints.MinHeight, constraints.MaxHeight)
            );
        }

        protected override void PerformPositioning()
        {
            var child = _state.Child;
            if (child == null)
            {
                ChildPosition = Vector2.zero;
                return;
            }

            var widget = Widget;
            var alignment = widget.Alignment;

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