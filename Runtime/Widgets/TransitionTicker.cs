using System;

namespace UniMob.UI.Widgets
{
    internal struct TransitionTicker<T>
    {
        private readonly IAnimation<T> _animation;
        private readonly Action<T> _updater;
        private bool _animating;

        public TransitionTicker(IAnimation<T> animation, Action<T> updater)
        {
            _animation = animation;
            _animating = true;
            _updater = updater;
            _updater(_animation.Value);

            Zone.Current.AddTicker(Update);
        }

        public void Dispose()
        {
            Zone.Current.RemoveTicker(Update);
        }

        private void Update()
        {
            var nextAnimating = _animation.IsAnimating;

            if (_animating || nextAnimating)
            {
                _updater(_animation.Value);
            }

            _animating = nextAnimating;
        }
    }
}