using UniMob.Core;
using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Navigator",
    typeof(RectTransform), typeof(NavigatorView), typeof(CanvasGroup))]

namespace UniMob.UI.Widgets
{
    internal class NavigatorView : View<INavigatorState>
    {
        private CanvasGroup canvasGroup;

        private ViewMapperBase _mapper;

        private Atom<object> _interactableReaction;

        protected override void Awake()
        {
            base.Awake();

            canvasGroup = GetComponent<CanvasGroup>();

            _interactableReaction = Atom.Computed(ViewLifetime, () =>
            {
                canvasGroup.interactable = State.Interactable;
                return default(object);
            }, keepAlive: true);
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(transform, link: false);

            _interactableReaction.Get();
        }

        protected override void Deactivate()
        {
            _interactableReaction.Deactivate();

            base.Deactivate();
        }

        protected override void Render()
        {
            using (var render = _mapper.CreateRender())
            {
                var children = State.Screens;
                foreach (var child in children)
                {
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

    public interface INavigatorState : IViewState
    {
        IState[] Screens { get; }

        bool Interactable { get; }
    }
}