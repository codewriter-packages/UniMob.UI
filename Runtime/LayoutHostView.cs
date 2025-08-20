using UniMob.UI.Internal;
using UniMob.UI.Layout;
using UnityEngine;

[assembly:
    RegisterComponentViewFactory("$$_LayoutHostView", typeof(RectTransform), typeof(UniMob.UI.Widgets.LayoutHostView))]

namespace UniMob.UI.Widgets
{
    // This view is the second half of the translator.
    internal class LayoutHostView : View<LayoutHostState>
    {
        private ViewMapperBase _mapper;

        protected override void Awake()
        {
            base.Awake();
            _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            var childState = State.Child;
            if (childState == null) return;
            
            
            var size = rectTransform.rect.size;
            var constraints = LayoutConstraints.Tight(size.x, size.y);

            
            childState.UpdateConstraints(constraints);
            
            // 3. Render the child. It now has the correct constraints and will lay out perfectly.
            using (var render = _mapper.CreateRender())
            {
                var childView = render.RenderItem(childState);
                
                // Ensure the child view fills the host.
                if (childView.rectTransform.anchorMin != Vector2.zero || childView.rectTransform.anchorMax != Vector2.one)
                {
                    childView.rectTransform.anchorMin = Vector2.zero;
                    childView.rectTransform.anchorMax = Vector2.one;
                    childView.rectTransform.sizeDelta = Vector2.zero;
                    childView.rectTransform.anchoredPosition = Vector2.zero;
                }
            }
        }
    }
}