using UniMob.UI.Internal;
using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;
using UnityEngine.UI;



[assembly: RegisterComponentViewFactory("$$_Layout.AlignView",
    typeof(UnityEngine.RectTransform), typeof(UnityEngine.CanvasRenderer), typeof(
        UniMob.UI.Layout.Internal.Views.AlignView))]


namespace UniMob.UI.Layout.Internal.Views
{
    internal class AlignView : SingleChildLayoutView<AlignState>{}
    
    
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public abstract class SingleChildLayoutView<TState> : View<TState>
        where TState : class, ISingleChildLayoutState
    {
        private ViewMapperBase _mapper;

        protected override void Awake()
        {
            base.Awake();
            _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            if (State.RenderObject is not ISingleChildRenderObject renderObject)
                return;

            // 3. Render the child
            using (var render = _mapper.CreateRender())
            {
                var child = State.Child;
                if (child == null)
                    return;

                var childView = render.RenderItem(child);

                var rt = childView.rectTransform;
                rt.anchorMin = Vector2.up;
                rt.anchorMax = Vector2.up;
                
                var childSize = renderObject.ChildSize;
                var topLeftPosition = renderObject.ChildPosition;

                // 3. We must translate this top-left position into the PIVOT's position,
                //    which is what RectTransform.anchoredPosition requires.
                var pivotOffset = new Vector2(
                    childSize.x * rt.pivot.x,
                    -childSize.y * (1.0f - rt.pivot.y)
                );

                // 4. Apply the final values to the RectTransform.
                rt.sizeDelta = childSize;
                rt.anchoredPosition = new Vector2(topLeftPosition.x, -topLeftPosition.y) + pivotOffset;
            }
        }
    }
}