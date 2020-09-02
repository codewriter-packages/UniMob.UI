using JetBrains.Annotations;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public class CompositeTransition : SingleChildLayoutWidget
    {
        private static readonly ConstAnimation<float> NormalOpacity = 1f;
        private static readonly ConstAnimation<Vector2> NormalPosition = Vector2.zero;
        private static readonly ConstAnimation<Vector3> NormalScale = Vector3.one;
        private static readonly ConstAnimation<Quaternion> NormalRotation = Quaternion.identity;

        public CompositeTransition(
            Widget child,
            [CanBeNull] IAnimation<float> opacity = null,
            [CanBeNull] IAnimation<Vector2> position = null,
            [CanBeNull] IAnimation<Vector3> scale = null,
            [CanBeNull] IAnimation<Quaternion> rotation = null,
            [CanBeNull] Key key = null
        ) : base(child, key)
        {
            Opacity = opacity ?? NormalOpacity;
            Position = position ?? NormalPosition;
            Scale = scale ?? NormalScale;
            Rotation = rotation ?? NormalRotation;
        }

        public IAnimation<float> Opacity { get; }
        public IAnimation<Vector2> Position { get; }
        public IAnimation<Vector3> Scale { get; }
        public IAnimation<Quaternion> Rotation { get; }

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