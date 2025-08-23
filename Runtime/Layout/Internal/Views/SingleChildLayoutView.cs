using UniMob.UI.Internal;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Internal.Views;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Layout.AlignView",
    typeof(RectTransform), typeof(CanvasRenderer), typeof(
        AlignView))]

[assembly: RegisterComponentViewFactory("$$_Layout.SizedBoxView",
    typeof(RectTransform), typeof(CanvasRenderer), typeof(
        SizedBoxView))]

[assembly: RegisterComponentViewFactory("$$_Layout.PaddingBoxView",
    typeof(RectTransform), typeof(CanvasRenderer), typeof(
        UniMob.UI.Layout.Internal.Views.PaddingBoxView))]

[assembly: RegisterComponentViewFactory("$$_Layout.ConstrainedBoxView",
    typeof(RectTransform), typeof(CanvasRenderer), typeof(
        ConstrainedBoxView))]

namespace UniMob.UI.Layout.Internal.Views
{
    internal class AlignView : SingleChildLayoutView<AlignState>
    {
    }
    internal class SizedBoxView : SingleChildLayoutView<SizedBoxState>
    {
    }

    internal class PaddingBoxView : SingleChildLayoutView<PaddingBoxState>
    {
    }
    
    internal class ConstrainedBoxView : SingleChildLayoutView<ConstrainedBoxState>
    {
    }


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

                var pivotOffset = new Vector2(
                    childSize.x * rt.pivot.x,
                    -childSize.y * (1.0f - rt.pivot.y)
                );

                rt.sizeDelta = childSize;
                rt.anchoredPosition = new Vector2(topLeftPosition.x, -topLeftPosition.y) + pivotOffset;
            }
        }
    }
}