using UniMob.UI.Internal;
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

                var childView = render.RenderItem(child);
                var childSize = child.Size;

                LayoutData layout;
                layout.Size = childSize.GetSizeUnbounded();
                layout.Alignment = Alignment.Center;
                layout.Corner = Alignment.Center;
                layout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
            }
        }
    }
}