using System;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class UniMobButton : SingleChildLayoutWidget
    {
        public bool Interactable { get; set; } = true;

        public Action OnClick { get; set; }

        public override State CreateState() => new UniMobButtonState();
    }

    public class UniMobButtonState : SingleChildLayoutState<UniMobButton>, IUniMobButtonState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.Button");

        public Alignment Alignment { get; } = Alignment.Center;

        public bool Interactable => Widget.Interactable;

        public void OnClick()
        {
            using (Atom.NoWatch)
            {
                Widget.OnClick?.Invoke();
            }
        }
    }
}