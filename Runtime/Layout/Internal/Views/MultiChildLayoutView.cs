using System;
using UniMob.UI.Internal;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Internal.Views;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Layout.MultiChildLayoutView",
    typeof(RectTransform),
    typeof(MultiChildLayoutView))]

namespace UniMob.UI.Layout.Internal.Views
{
    public interface IMultiChildLayoutState : ILayoutState
    {
        IState[] Children { get; }
    }

    public class MultiChildLayoutView : View<IMultiChildLayoutState>
    {
        private ViewMapperBase _mapper;

        protected override void Awake()
        {
            base.Awake();
            _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            if (State.RenderObject is not IMultiChildRenderObject multiChildRenderObject)
                return;

            var childrenLayout = multiChildRenderObject.ChildrenLayout;

            using var render = _mapper.CreateRender();
            // Render each child
            for (var i = 0; i < State.Children.Length; i++)
            {
                var child = State.Children[i];
                if (child is ExpandedState expandedState) child = expandedState.Child;


                var layoutData = childrenLayout[i];

                if (float.IsInfinity(layoutData.Size.x) || float.IsInfinity(layoutData.Size.y))
                    throw new InvalidOperationException(
                        $"Child {child.GetType().Name} at position {i} has an unbounded size. " +
                        "This is not supported in MultiChildLayoutView. " +
                        "Try wrapping it in a Container or similar widget to constrain its size.");


                var childView = render.RenderItem(child);
                var rt = childView.rectTransform;

                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);

                var pivotOffset = new Vector2(
                    layoutData.Size.x * rt.pivot.x,
                    -layoutData.Size.y * (1.0f - rt.pivot.y)
                );

                rt.sizeDelta = layoutData.Size;
                if (!layoutData.CornerPosition.HasValue)
                    throw new InvalidOperationException(
                        $"LayoutData for child {i} does not have a CornerPosition set. This is required for positioning.");
                rt.anchoredPosition =
                    new Vector2(layoutData.CornerPosition.Value.x, -layoutData.CornerPosition.Value.y) + pivotOffset;
            }
        }
    }
}