using System;
using UnityEngine;


namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderContainer : RenderObject, ISingleChildRenderObject
    {
        private readonly Container _widget;
        private readonly ContainerState _state;
        
        // ADD THIS: A place to store the final result of the positioning pass.
        public Vector2 ChildPosition { get; private set; }
        public Vector2 ChildSize { get; private set; }

        public RenderContainer(Container widget, ContainerState state)
        {
            _widget = widget;
            _state = state;
        }
        
        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var childContraints  = new LayoutConstraints(
                0, // A child can always choose to be smaller.
                0,
                _widget.Width ?? constraints.MaxWidth,   // Use own width if set, otherwise parent's.
                _widget.Height ?? constraints.MaxHeight  // Use own height if set, otherwise parent's.
            );

            // 2. The child's size is now calculated cleanly using the helper,
            //    but with the CORRECT, TIGHTENED constraints.
            this.ChildSize = LayoutChild(_state.Child, childContraints);

            // 2. SIZING PASS: Determine this container's own size.
            float finalWidth = _widget.Width ?? (float.IsInfinity(constraints.MaxWidth) ? this.ChildSize.x : constraints.MaxWidth);
            float finalHeight = _widget.Height ?? (float.IsInfinity(constraints.MaxHeight) ? this.ChildSize.y : constraints.MaxHeight);

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

            var alignment = _widget.Alignment;

            float x = (this.Size.x - this.ChildSize.x) * (alignment.X * 0.5f + 0.5f);
            float y = (this.Size.y - this.ChildSize.y) * (alignment.Y * 0.5f + 0.5f);

            this.ChildPosition = new Vector2(x, y);
        }

        public override float GetIntrinsicWidth(float height)
        {
            if (this._widget.Width.HasValue) return this._widget.Width.Value;

            if (_state.Child is ILayoutState childLayoutState)
            {
                return childLayoutState.RenderObject.GetIntrinsicWidth(height);
            }
            
            // Fallback for legacy widgets
            return _state.Child?.Size.GetSizeUnbounded().x ?? 0;
        }

        public override float GetIntrinsicHeight(float width)
        {
            if (this._widget.Height.HasValue) return this._widget.Height.Value;

            if (_state.Child is ILayoutState childLayoutState)
            {
                return childLayoutState.RenderObject.GetIntrinsicHeight(width);
            }

            // Fallback for legacy widgets
            return _state.Child?.Size.GetSizeUnbounded().y ?? 0;
        }
    }
}