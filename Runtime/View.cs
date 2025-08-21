using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniMob.Core;
using UniMob.UI.Layout;
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

        private Atom<Vector2Int> _bounds;

        [CanBeNull] private List<Action> _activationCallbacks;
        [CanBeNull] private List<Action> _deactivationCallbacks;

        private LifetimeController _stateLifetimeController;

        private TState _currentState;
        private TState _nextState;

        private Atom<TState> _doRebind;
        private Atom<object> _doRender;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private CustomSampler _renderSampler;
#endif
        public bool HasState => _currentState != null;
        protected TState State => _currentState;

        internal virtual bool TriggerViewMountEvents => true;

        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform => (RectTransform) transform;

        bool IView.IsDestroyed => this == null;

        protected Vector2Int Bounds
        {
            get
            {
                if (_bounds == null)
                {
                    _bounds = Atom.Computed(ViewLifetime, () =>
                    {
                        var size = rectTransform.rect.size;
                        return new Vector2Int((int) size.x, (int) size.y);
                    }, debugName: "View._bounds");
                }

                return _bounds.Value;
            }
        }

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

        void IView.SetSource(IViewState newSource, bool link)
        {
            if (_doRebind == null)
            {
                _doRebind = Atom.Computed(ViewLifetime, DoRebind, keepAlive: true, debugName: $"View._doRebind");
                _doRender = Atom.Computed(ViewLifetime, DoRender, keepAlive: true, debugName: $"View._doRender");
            }

            _renderScope.Link(this);

            var doRebindAtom = ((AtomBase) _doRebind);

            if (!ReferenceEquals(newSource, _currentState))
            {
                if (newSource is TState typedState)
                {
                    _nextState = typedState;
                }
                else
                {
                    var expected = typeof(TState).Name;
                    var actual = newSource.GetType().Name;
                    Debug.LogError($"Wrong model type at '{name}': expected={expected}, actual={actual}");
                    return;
                }

                doRebindAtom.Actualize(true);
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
                        if (TriggerViewMountEvents)
                        {
                            _currentState.DidViewUnmount(this);
                        }
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }

                    try
                    {
                        Deactivate();
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }
                }
            }

            foreach (var child in _children)
            {
                child.Unmount();
            }

            _nextState = null;
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
            if (ReferenceEquals(_currentState, _nextState))
            {
                _doRender.Get();
                return _nextState;
            }

            using (Atom.NoWatch)
            {
                if (_currentState != null)
                {
                    try
                    {
                        if (TriggerViewMountEvents)
                        {
                            _currentState.DidViewUnmount(this);
                        }
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }

                    try
                    {
                        Deactivate();
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }
                }

                _currentState = _nextState;

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

            using (Atom.NoWatch)
            {
                try
                {
                    if (TriggerViewMountEvents)
                    {
                        _currentState.DidViewMount(this);
                    }
                }
                catch (Exception ex)
                {
                    Zone.Current.HandleUncaughtException(ex);
                }
            }

            return _nextState;
        }

        private object DoRender()
        {
            var currentState = _currentState;

            if (currentState == null)
            {
                return null;
            }

            if (currentState.AsLayoutState(out var layoutState))
            {
                try
                {
                    // We MUST perform layout here so that the inheritors in the Render() method
                    // receive a fully up-to-date RenderObject and can use its properties, even non-reactive ones.
                    //
                    // Also we MUST do a DoRender() if the layout is recomputed, so subscribe to it.
                    layoutState.WatchedPerformLayout();
                }
                catch (Exception ex)
                {
                    Zone.Current.HandleUncaughtException(ex);
                }
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

        private void RefreshBounds()
        {
            if (_bounds == null)
            {
                return;
            }

            using (Atom.NoWatch)
            {
                _bounds.Invalidate();
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            RefreshBounds();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            RefreshBounds();
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