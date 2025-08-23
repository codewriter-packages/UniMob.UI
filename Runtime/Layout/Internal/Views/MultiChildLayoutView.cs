using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UniMob.UI.Internal;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Widgets;

[assembly: UniMob.UI.Internal.RegisterComponentViewFactory("$$_Layout.MultiChildLayoutView", 
    typeof(UnityEngine.RectTransform), 
    typeof(UniMob.UI.Layout.Views.MultiChildLayoutView))]

namespace UniMob.UI.Layout.Views
{
    public interface IMultiChildLayoutState : IState
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
                
                var layoutData = childrenLayout[i];
                
                if(float.IsInfinity(layoutData.Size.x) || float.IsInfinity(layoutData.Size.y))
                {
                    throw new InvalidOperationException(
                        $"Child {i} has an unbounded size. This is not supported in MultiChildLayoutView.");
                }
                
                
                
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
                {
                    throw new InvalidOperationException(
                        $"LayoutData for child {i} does not have a CornerPosition set. This is required for positioning.");
                }
                rt.anchoredPosition = new Vector2(layoutData.CornerPosition.Value.x, -layoutData.CornerPosition.Value.y) + pivotOffset;
            }
        }
    }
}