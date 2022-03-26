using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UniMob.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ViewBindable<TState> : UIBehaviour, IView, IViewTreeElement
        where TState : class, IViewState
    {
        private readonly List<Action> _activationCallbacks = new List<Action>();
        private readonly List<Action> _deactivationCallbacks = new List<Action>();

        private TState _currentState;

        public RectTransform rectTransform => (RectTransform) transform;

        WidgetViewReference IView.ViewReference { get; set; }

        bool IView.IsDestroyed => this == null;

        protected TState State => _currentState;
        protected bool HasState => _currentState != null;

        public void AddActivationCallback(Action callback)
        {
            _activationCallbacks.Add(callback);
        }

        public void AddDeactivationCallback(Action callback)
        {
            _deactivationCallbacks.Add(callback);
        }

        public void RemoveActivationCallback(Action callback)
        {
            _activationCallbacks.Remove(callback);
        }

        public void RemoveDeactivationCallback(Action callback)
        {
            _deactivationCallbacks.Remove(callback);
        }

        private void Unmount()
        {
            if (_currentState != null)
            {
                DidStateDetached(_currentState);
            }

            _currentState = null;
        }

        void IView.SetSource(IViewState source, bool link)
        {
            if (_currentState == source)
            {
                return;
            }

            if (source is TState typedState)
            {
                if (_currentState != null)
                {
                    DidStateDetached(_currentState);
                }

                _currentState = typedState;

                if (_currentState != null)
                {
                    DidStateAttached(_currentState);
                }
            }
            else
            {
                var expected = typeof(TState).Name;
                var actual = source.GetType().Name;
                Debug.LogError($"Wrong model type at '{name}': expected={expected}, actual={actual}");
            }
        }

        void IView.ResetSource()
        {
            Unmount();
        }

        void IViewTreeElement.AddChild(IViewTreeElement view)
        {
            throw new InvalidOperationException($"{nameof(ViewBindable<TState>)} must not be used as ViewTree root");
        }

        void IViewTreeElement.Unmount()
        {
            Unmount();
        }

        private void DidStateAttached(TState state)
        {
            try
            {
                using (Atom.NoWatch)
                {
                    state.DidViewMount(this);

                    foreach (var callback in _activationCallbacks)
                    {
                        callback.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Zone.Current.HandleUncaughtException(ex);
            }
        }

        private void DidStateDetached(TState state)
        {
            try
            {
                using (Atom.NoWatch)
                {
                    state.DidViewUnmount(this);

                    foreach (var callback in _deactivationCallbacks)
                    {
                        callback.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Zone.Current.HandleUncaughtException(ex);
            }
        }
    }
}