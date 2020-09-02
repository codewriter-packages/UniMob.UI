using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class FadeTransition : SingleChildLayoutWidget
    {
        public IAnimation<float> Opacity { get; set; } = new ConstAnimation<float>(1f);

        public override State CreateState() => new FadeTransitionState();
    }

    internal class FadeTransitionState : SingleChildLayoutState<FadeTransition>, IFadeTransitionState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_FadeTransition");

        public IAnimation<float> Opacity => Widget.Opacity;
        public Alignment Alignment => Alignment.Center;
    }
}