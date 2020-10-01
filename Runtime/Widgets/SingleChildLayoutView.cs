using UniMob.UI.Internal;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    internal class SingleChildLayoutView<TState> : View<TState>
        where TState : ISingleChildLayoutState
    {
        private ViewMapperBase _mapper;

        protected IView ChildView { get; private set; }

        protected override void Activate()
        {
            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);

            base.Activate();
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            ChildView = null;
        }

        protected override void Render()
        {
            var alignment = State.Alignment;

            using (var render = _mapper.CreateRender())
            {
                var child = State.Child;
                ChildView = render.RenderItem(child);
                var childSize = child.Size.GetSizeUnbounded();

                LayoutData layout;
                layout.Size = childSize;
                layout.Alignment = alignment;
                layout.Corner = alignment;
                layout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(ChildView.rectTransform, layout);
            }
        }
    }

    internal interface ISingleChildLayoutState : IViewState
    {
        Alignment Alignment { get; }
        IState Child { get; }
    }
}