using UniMob.UI.Internal;
using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;


namespace UniMob.UI.Layout.Internal.Views
{
    /// <summary>
    /// Represents a base class for views that manage a single child layout, providing functionality to render and
    /// position a single child element within a layout container.
    /// </summary>
    /// <remarks>This class is designed to work with a single child layout model, where the layout state
    /// defines the size and position of the child element.  It requires the associated GameObject to have a <see
    /// cref="RectTransform"/>  and a <see cref="CanvasRenderer"/> component.  The layout logic ensures that the child
    /// element is positioned and sized  according to the layout state provided by the <typeparamref
    /// name="TState"/>.</remarks>
    /// <typeparam name="TState">The type of the state object associated with the view </typeparam> 
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