using UniMob.UI.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    internal sealed class DismissibleDialogView : View<IDismissibleDialogState>
    {
        [SerializeField] private UniMobDismissibleDialogBehaviour dismissibleDialogBehaviour = default;
        [SerializeField] private Button backgroundButton = default;

        private ViewMapperBase _mapper;

        protected override void Awake()
        {
            base.Awake();

            backgroundButton.Click(() => State.OnDismiss);
        }

        protected override void Activate()
        {
            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);

            dismissibleDialogBehaviour.State = State;

            base.Activate();

            Atom.Reaction(StateLifetime,
                () => Bounds.y,
                v => State.SetExpandedHeight(v),
                fireImmediately: true);
        }

        protected override void Deactivate()
        {
            dismissibleDialogBehaviour.State = null;

            base.Deactivate();
        }

        protected override void Render()
        {
            using (var render = _mapper.CreateRender())
            {
                var child = State.Child;
                var childView = render.RenderItem(child);
                var childSize = State.Child.Size.GetSize(State.ChildSize.GetSizeUnbounded());

                LayoutData layout;
                layout.Size = childSize;
                layout.Alignment = Alignment.BottomCenter;
                layout.Corner = Alignment.BottomCenter;
                layout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
            }
        }
    }

    internal interface IDismissibleDialogState : IViewState
    {
        float Offset { get; }
        float DismissThreshold { get; }
        bool Expanded { get; }
        IState Child { get; }
        WidgetSize ChildSize { get; }

        void SetOffset(float offset);
        void SetExpandedHeight(float height);
        void OnExpand();
        void OnCollapse();
        void OnDismiss();
    }
}