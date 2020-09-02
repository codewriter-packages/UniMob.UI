using JetBrains.Annotations;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public class CompositeTransition : SingleChildLayoutWidget
    {
        public IAnimation<float> Opacity { get; set; } = new ConstAnimation<float>(1f);
        public IAnimation<Vector2> Position { get; set; } = new ConstAnimation<Vector2>(Vector2.zero);
        public IAnimation<Vector3> Scale { get; set; } = new ConstAnimation<Vector3>(Vector3.one);
        public IAnimation<Quaternion> Rotation { get; set; } = new ConstAnimation<Quaternion>(Quaternion.identity);

        public override State CreateState() => new CompositeTransitionState();
    }

    internal class CompositeTransitionState : SingleChildLayoutState<CompositeTransition>, ICompositeTransitionState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_CompositeTransition");

        public IAnimation<float> Opacity => Widget.Opacity;
        public IAnimation<Vector2> Position => Widget.Position;
        public IAnimation<Vector3> Scale => Widget.Scale;
        public IAnimation<Quaternion> Rotation => Widget.Rotation;
        public Alignment Alignment => Alignment.Center;
    }
}