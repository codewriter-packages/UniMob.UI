using System;
using System.Collections.Generic;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderZStack : RenderObject, IMultiChildRenderObject
    {
        private readonly ZStackState _state;

        private readonly List<LayoutData> _childrenLayout = new();
        public IReadOnlyList<LayoutData> ChildrenLayout => _childrenLayout;

        public ZStack Widget => (ZStack) _state.RawWidget;

        public RenderZStack(ZStackState state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            _childrenLayout.Clear();
            float maxWidth = 0f;
            float maxHeight = 0f;

            foreach (var child in _state.Children)
            {
                // ZStack passes its own constraints down to every child.
                var childSize = LayoutChild(child, constraints);
                _childrenLayout.Add(new LayoutData { Size = childSize });

                maxWidth = Mathf.Max(maxWidth, childSize.x);
                maxHeight = Mathf.Max(maxHeight, childSize.y);
            }

            // The final size of the ZStack is the size of its largest child,
            // constrained by the parent's limits.
            return new Vector2(
                Mathf.Clamp(maxWidth, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(maxHeight, constraints.MinHeight, constraints.MaxHeight)
            );
        }

        protected override void PerformPositioning()
        {
            var widget = this.Widget;

            for (var i = 0; i < _childrenLayout.Count; i++)
            {
                var layout = _childrenLayout[i];
                var alignment = widget.Alignment;

                var x = (this.Size.x - layout.Size.x) * (alignment.X * 0.5f + 0.5f);
                var y = (this.Size.y - layout.Size.y) * (alignment.Y * 0.5f + 0.5f);
                
                var newLayoutData = _childrenLayout[i];
                newLayoutData.CornerPosition = new Vector2(x, y);
                _childrenLayout[i] = newLayoutData;
            }
        }

        public override float GetIntrinsicWidth(float height)
        {
            float maxWidth = 0;
            foreach (var child in _state.Children)
            {
                maxWidth = Mathf.Max(maxWidth, child.RenderObject.GetIntrinsicWidth(height));
            }
            return maxWidth;
        }

        public override float GetIntrinsicHeight(float width)
        {
            float maxHeight = 0;
            foreach (var child in _state.Children)
            {
                maxHeight = Mathf.Max(maxHeight, child.RenderObject.GetIntrinsicHeight(width));
            }
            return maxHeight;
        }


        
    }
}