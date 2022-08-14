using UnityEngine;

namespace UniMob.UI
{
    public class TabController : ILifetimeScope
    {
        private float _prevValue;
        private float _elapsed;

        [Atom] public float Value { get; private set; }
        [Atom] public int Index { get; private set; }
        [Atom] public int PreviousIndex { get; private set; }
        [Atom] public bool IndexIsChanging { get; private set; }

        public Lifetime Lifetime { get; }

        public float Duration { get; }
        public int TabCount { get; }

        public TabController(Lifetime lifetime, int tabCount, float duration)
        {
            Lifetime = lifetime;
            TabCount = tabCount;
            Duration = duration;
        }

        public void SetValue(float value)
        {
            RemoveAnimationTicker();

            Value = Mathf.Clamp(value, 0, TabCount - 1);
        }

        public void AnimateTo(int newIndex)
        {
            using (Atom.NoWatch)
            {
                _prevValue = Value;
                _elapsed = 0f;

                PreviousIndex = Index;
                Index = newIndex;
                IndexIsChanging = true;

                if (Mathf.Approximately(Duration, 0f))
                {
                    IndexIsChanging = false;
                }
                else
                {
                    AddAnimationTicker();
                }
            }
        }

        private void AddAnimationTicker()
        {
            Zone.Current.RemoveTicker(Tick);
            Zone.Current.AddTicker(Tick);
        }

        private void RemoveAnimationTicker()
        {
            Zone.Current.RemoveTicker(Tick);
        }

        private void Tick()
        {
            if (Lifetime.IsDisposed)
            {
                RemoveAnimationTicker();
                return;
            }

            _elapsed += Time.unscaledDeltaTime;

            Value = Mathf.Lerp(_prevValue, Index, _elapsed / Duration);

            if (!Mathf.Approximately(Value, Index))
            {
                return;
            }

            Value = Index;
            IndexIsChanging = false;

            RemoveAnimationTicker();
        }
    }
}