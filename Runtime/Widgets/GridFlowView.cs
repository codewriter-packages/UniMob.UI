using System;
using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Grid", typeof(RectTransform), typeof(GridFlowView))]

namespace UniMob.UI.Widgets
{
    internal class GridFlowView : View<IGridFlowState>
    {
        private ViewMapperBase _mapper;

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            var children = State.Children;
            var crossAxis = State.CrossAxisAlignment;
            var mainAxis = State.MainAxisAlignment;
            var gridSize = State.InnerSize.GetSize(Bounds);

            var alignX = crossAxis == CrossAxisAlignment.Start ? Alignment.TopLeft.X
                : crossAxis == CrossAxisAlignment.End ? Alignment.TopRight.X
                : Alignment.Center.X;

            var alignY = mainAxis == MainAxisAlignment.Start ? Alignment.TopCenter.Y
                : mainAxis == MainAxisAlignment.End ? Alignment.BottomCenter.Y
                : Alignment.Center.Y;

            var offsetMultiplierX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                : crossAxis == CrossAxisAlignment.End ? 1.0f
                : 0.5f;

            var offsetMultiplierY = mainAxis == MainAxisAlignment.Start ? 0.0f
                : mainAxis == MainAxisAlignment.End ? 1.0f
                : 0.5f;

            var childAlignment = new Alignment(alignX, alignY);
            var cornerPosition = new Vector2(
                -gridSize.x * offsetMultiplierX,
                -gridSize.y * offsetMultiplierY);

            using (var render = _mapper.CreateRender())
            {
                var newLine = true;
                var lineLastChildIndex = 0;
                var lineHeight = 0f;
                var lineMaxWidth = State.MaxCrossAxisExtent;
                var lineMaxChildCount = State.MaxCrossAxisCount;

                for (var childIndex = 0; childIndex < children.Length; childIndex++)
                {
                    var child = children[childIndex];
                    var childSize = child.Size.GetSize(Bounds);

                    if (newLine)
                    {
                        newLine = false;
                        lineHeight = childSize.y;
                        var lineWidth = childSize.x;
                        var lineChildCount = 1;

                        for (int i = childIndex + 1; i < children.Length; i++)
                        {
                            var nextChildSize = children[i].Size.GetSize(Bounds);
                            if (lineChildCount + 1 <= lineMaxChildCount &&
                                lineWidth + nextChildSize.x <= lineMaxWidth)
                            {
                                lineChildCount++;
                                lineWidth += nextChildSize.x;
                                lineHeight = Math.Max(lineHeight, nextChildSize.y);
                            }
                            else
                            {
                                break;
                            }
                        }

                        lineLastChildIndex = childIndex + lineChildCount - 1;
                        cornerPosition.x = -lineWidth * offsetMultiplierX;
                    }

                    var childView = render.RenderItem(child);

                    LayoutData layout;
                    layout.Size = childSize;
                    layout.Alignment = childAlignment;
                    layout.Corner = childAlignment.WithLeft();
                    layout.CornerPosition = cornerPosition + new Vector2(0, lineHeight * offsetMultiplierY);
                    ViewLayoutUtility.SetLayout(childView.rectTransform, layout);

                    if (childIndex == lineLastChildIndex)
                    {
                        newLine = true;
                        cornerPosition.y += lineHeight;
                    }
                    else
                    {
                        cornerPosition.x += childSize.x;
                    }
                }
            }
        }
    }

    internal interface IGridFlowState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        int MaxCrossAxisCount { get; }
        float MaxCrossAxisExtent { get; }
    }
}