using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_AnimatedCrossFade",
    typeof(RectTransform), typeof(AnimatedCrossFadeView))]

namespace UniMob.UI.Widgets
{
    internal class AnimatedCrossFadeView : View<IAnimatedCrossFadeState>
    {
        private ViewMapperBase _mapper;

        protected override void Activate()
        {
            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);

            base.Activate();
        }

        protected override void Render()
        {
            var alignment = State.Alignment;

            using (var render = _mapper.CreateRender())
            {
                RenderChild(render, State.FirstChild, alignment);
                RenderChild(render, State.SecondChild, alignment);
            }
        }

        private void RenderChild(ViewMapperBase.ViewMapperRenderScope render, IState child, Alignment alignment)
        {
            var childView = render.RenderItem(child);
            var childSize = child.Size;

            LayoutData layout;
            layout.Size = childSize;
            layout.Alignment = alignment;
            layout.Corner = alignment;
            layout.CornerPosition = Vector2.zero;
            ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
        }
    }

    internal interface IAnimatedCrossFadeState : IViewState
    {
        IState FirstChild { get; }
        IState SecondChild { get; }
        Alignment Alignment { get; }
    }
}