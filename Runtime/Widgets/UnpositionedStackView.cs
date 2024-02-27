using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_UnPositionedStack",
    typeof(RectTransform), typeof(UnPositionedStackView))]

namespace UniMob.UI.Widgets
{
    internal class UnPositionedStackView : View<IUnPositionedStackState>
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

            using (var render = _mapper.CreateRender())
            {
                foreach (var child in children)
                {
                    render.RenderItem(child);
                }
            }
        }
    }

    internal interface IUnPositionedStackState : IViewState
    {
        IState[] Children { get; }
    }
}