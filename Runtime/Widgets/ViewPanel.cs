using UniMob.UI.Internal;
using UniMob.UI.Layout;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    [AddComponentMenu("UniMob/Views/ViewPanel")]
    public sealed class ViewPanel : View<IViewState>
    {
        private ViewMapperBase _mapper;

        internal override bool TriggerViewMountEvents => false;

        public void Render(IState state, bool link = false)
        {
            base.Render(state.InnerViewState, link);
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            using (var render = _mapper.CreateRender())
            {
                var child = State;

                Vector2 finalSize;
                if (child is ILayoutState layoutState)
                {
                    // If the root widget is layout-aware, we MUST use its RenderObject's size.
                    // The PerformLayout() call was already handled in the base View.DoRender().
                    finalSize = layoutState.RenderObject.Size;
                }
                else
                {
                    // Fallback for a legacy root widget.
                    finalSize = child.Size.GetSizeUnbounded();
                }
                
                var childView = render.RenderItem(child);
                

                LayoutData layout;
                layout.Size = finalSize;
                layout.Alignment = Alignment.Center;
                layout.Corner = Alignment.Center;
                layout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
            }
        }
    }
}