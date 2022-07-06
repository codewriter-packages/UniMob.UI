using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniMob.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

namespace UniMob.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class View<TState> : UIBehaviour, IView, IViewTreeElement
        where TState : class, IViewState
    {
        [NotNull] private readonly ViewRenderScope _renderScope = new ViewRenderScope();
        [NotNull] private readonly List<IViewTreeElement> _children = new List<IViewTreeElement>();
        private readonly LifetimeController _viewLifetimeController = new LifetimeController();

        private readonly MutableAtom<Vector2Int> _bounds = Atom.Value(Vector2Int.zero, debugName: "View.bounds");

        [CanBeNull] private List<Action> _activationCallbacks;
        [CanBeNull] private List<Action> _deactivationCallbacks;

        private LifetimeController _stateLifetimeController;
        private Action _refreshBoundsAction;

        private TState _currentState;

        private MutableAtom<TState> _nextState;
        private Atom<TState> _doRebind;
        private Atom<object> _doRender;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private CustomSampler _renderSampler;
#endif
        public bool HasState => _currentState != null;
        protected TState State => _currentState;

        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform => (RectTransform) transform;

        WidgetViewReference IView.ViewReference { get; set; }

        bool IView.IsDestroyed => this == null;

        protected Vector2Int Bounds => _bounds.Value;

        public Lifetime ViewLifetime => _viewLifetimeController.Lifetime;

        protected Lifetime StateLifetime
        {
            get
            {
                if (_stateLifetimeController == null)
                {
                    _stateLifetimeController = new LifetimeController();
                }

                return _stateLifetimeController.Lifetime;
            }
        }

        public void Render(TState state, bool link = false)
        {
            var self = (IView) this;
            self.SetSource(state, link);
        }

        private void Initialize()
        {
            if (_nextState != null)
            {
                return;
            }

            _nextState = Atom.Value<TState>(ViewLifetime, null, debugName: $"View<{typeof(TState)}>({name})::State");
            _doRebind = Atom.Computed(ViewLifetime, DoRebind, debugName: $"View<{typeof(TState)}>({name})::Bind()",
                keepAlive: true);
            _doRender = Atom.Computed(ViewLifetime, DoRender, debugName: $"View<{typeof(TState)}>({name})::Render()",
                keepAlive: true);
        }

        void IView.SetSource(IViewState newSource, bool link)
        {
            Initialize();

            _renderScope.Link(this);

            var doRebindAtom = ((AtomBase) _doRebind);

            if (!doRebindAtom.options.Has(AtomOptions.Active))
            {
                doRebindAtom.Actualize();
            }

            if (!ReferenceEquals(newSource, _currentState))
            {
                if (newSource is TState typedState)
                {
                    using (Atom.NoWatch)
                    {
                        _nextState.Value = typedState;
                    }
                }
                else
                {
                    var expected = typeof(TState).Name;
                    var actual = newSource.GetType().Name;
                    Debug.LogError($"Wrong model type at '{name}': expected={expected}, actual={actual}");
                    return;
                }

                RefreshBoundsImmediate();
                RefreshBoundsNextFrame();
            }

            if (link)
            {
                _doRebind.Get();
            }
            else
            {
                doRebindAtom.Actualize();
            }
        }

        private void Unmount()
        {
            using (Atom.NoWatch)
            {
                _doRebind?.Deactivate();
                _doRender?.Deactivate();

                if (_currentState != null)
                {
                    try
                    {
                        using (Atom.NoWatch)
                        {
                            Deactivate();
                        }
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }

                    DidStateDetached(_currentState);
                }
            }

            foreach (var child in _children)
            {
                child.Unmount();
            }

            if (_nextState != null)
            {
                using (Atom.NoWatch)
                {
                    _nextState.Value = null;
                }
            }

            _currentState = null;
        }

        void IView.ResetSource()
        {
            Unmount();
        }

        void IViewTreeElement.AddChild(IViewTreeElement view)
        {
            _children.Add(view);
        }

        void IViewTreeElement.Unmount()
        {
            Unmount();
        }

        private TState DoRebind()
        {
            Initialize();

            var nextState = _nextState.Value;

            using (Atom.NoWatch)
            {
                var currentState = _currentState;
                if (ReferenceEquals(currentState, nextState))
                {
                    return nextState;
                }

                if (currentState != null)
                {
                    DidStateDetached(currentState);

                    try
                    {
                        Deactivate();
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }
                }

                _currentState = nextState;

                try
                {
                    Activate();
                }
                catch (Exception ex)
                {
                    Zone.Current.HandleUncaughtException(ex);
                }
            }

            ((AtomBase) _doRender).Actualize(true);
            _doRender.Get();

            DidStateAttached(nextState);

            return nextState;
        }

        private object DoRender()
        {
            var currentState = _currentState;

            if (currentState == null)
            {
                return null;
            }

            using (_renderScope.Enter(this))
            {
                _children.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_renderSampler == null)
                {
                    _renderSampler = CustomSampler.Create($"Render {name}");
                }

                _renderSampler.Begin();
#endif

                try
                {
                    Render();
                }
                catch (Exception ex)
                {
                    Zone.Current.HandleUncaughtException(ex);
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _renderSampler.End();
#endif
            }

            return null;
        }

        private void RefreshBoundsNextFrame()
        {
            if (_refreshBoundsAction == null)
            {
                _refreshBoundsAction = RefreshBoundsImmediate;
            }

            Zone.Current.NextFrame(_refreshBoundsAction);
        }

        private void RefreshBoundsImmediate()
        {
            using (Atom.NoWatch)
            {
                var size = rectTransform.rect.size;
                _bounds.Value = new Vector2Int((int) size.x, (int) size.y);
            }
        }

        private void DidStateAttached(TState state)
        {
            try
            {
                using (Atom.NoWatch)
                {
                    state.DidViewMount(this);
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
                }
            }
            catch (Exception ex)
            {
                Zone.Current.HandleUncaughtException(ex);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            RefreshBoundsNextFrame();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            RefreshBoundsNextFrame();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _viewLifetimeController.Dispose();
        }

        protected virtual void Activate()
        {
            if (_activationCallbacks != null)
            {
                foreach (var call in _activationCallbacks)
                {
                    call.Invoke();
                }
            }
        }

        protected virtual void Deactivate()
        {
            _stateLifetimeController?.Dispose();
            _stateLifetimeController = null;

            if (_deactivationCallbacks != null)
            {
                foreach (var call in _deactivationCallbacks)
                {
                    call.Invoke();
                }
            }
        }

        protected abstract void Render();

        public void AddActivationCallback(Action callback)
        {
            if (_activationCallbacks == null)
            {
                _activationCallbacks = new List<Action>();
            }

            _activationCallbacks.Add(callback);
        }

        public void AddDeactivationCallback(Action callback)
        {
            if (_deactivationCallbacks == null)
            {
                _deactivationCallbacks = new List<Action>();
            }

            _deactivationCallbacks.Add(callback);
        }

        public void RemoveActivationCallback(Action callback)
        {
            _activationCallbacks?.Remove(callback);
        }

        public void RemoveDeactivationCallback(Action callback)
        {
            _deactivationCallbacks?.Remove(callback);
        }
    }
}