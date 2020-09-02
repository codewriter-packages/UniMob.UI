using UnityEngine;
using UIButton = UnityEngine.UI.Button;

namespace UniMob.UI.Widgets
{
    [RequireComponent(typeof(UIButton))]
    internal class UniMobButtonView : SingleChildLayoutView<IUniMobButtonState>
    {
        private UIButton _button;

        protected override void Awake()
        {
            base.Awake();

            _button = GetComponent<UIButton>();
            _button.onClick.AddListener(HandleClick);
        }

        protected override void Render()
        {
            base.Render();

            _button.interactable = State.Interactable;
        }

        private void HandleClick()
        {
            if (!HasState) return;

            State.OnClick();
        }
    }

    internal interface IUniMobButtonState : ISingleChildLayoutState
    {
        bool Interactable { get; }

        void OnClick();
    }
}