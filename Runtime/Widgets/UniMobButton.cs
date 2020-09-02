using System;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class UniMobButton : SingleChildLayoutWidget
    {
        public UniMobButton(
            [NotNull] Widget child,
            [NotNull] Action onClick,
            bool interactable = true,
            [CanBeNull] Key key = null
        ) : base(child, key)
        {
            OnClick = onClick;
            Interactable = interactable;
        }

        public bool Interactable { get; }

        public Action OnClick { get; }

        public override State CreateState() => new UniMobButtonState();
    }

    internal class UniMobButtonState : SingleChildLayoutState<UniMobButton>, IUniMobButtonState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.Button");

        public Alignment Alignment { get; } = Alignment.Center;

        public bool Interactable => Widget.Interactable;

        public void OnClick() => Widget.OnClick();
    }
}