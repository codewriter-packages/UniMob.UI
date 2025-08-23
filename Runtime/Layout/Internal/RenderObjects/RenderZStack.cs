using System.Collections.Generic;
using System.Linq;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderZStack : RenderObject, IMultiChildRenderObject
    {
        private readonly List<LayoutData> _childrenLayout = new();
        private readonly ZStackState _state;

        public IReadOnlyList<LayoutData> ChildrenLayout => _childrenLayout;

        public RenderZStack(ZStackState state)
        {
            _state = state;
        }


        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            _childrenLayout.Clear();
            var maxWidth = 0f;
            var maxHeight = 0f;

            foreach (var child in _state.Children)
            {
                // ZStack passes its own constraints down to every child.
                var childSize = LayoutChild(child, constraints);
                _childrenLayout.Add(new LayoutData {Size = childSize});

                maxWidth = Mathf.Max(maxWidth, childSize.x);
                maxHeight = Mathf.Max(maxHeight, childSize.y);
            }

            // The final size of the ZStack is the size of its largest child,
            // constrained by the parent's limits.
            return constraints.Constrain(new Vector2(maxWidth, maxHeight));
        }

        protected override void PerformPositioning()
        {
            var widget = _state.RawWidget as ZStack;

            for (var i = 0; i < _childrenLayout.Count; i++)
            {
                var layout = _childrenLayout[i];
                var alignment = widget.Alignment;

                var x = (Size.x - layout.Size.x) * (alignment.X * 0.5f + 0.5f);
                var y = (Size.y - layout.Size.y) * (alignment.Y * 0.5f + 0.5f);

                var newLayoutData = _childrenLayout[i];
                newLayoutData.CornerPosition = new Vector2(x, y);
                _childrenLayout[i] = newLayoutData;
            }
        }

        public override float GetIntrinsicWidth(float height)
        {
            return _state.Children.Max(child => child.RenderObject.GetIntrinsicWidth(height));
        }

        public override float GetIntrinsicHeight(float width)
        {
            return _state.Children.Max(child => child.RenderObject.GetIntrinsicHeight(width));
        }
    }
}