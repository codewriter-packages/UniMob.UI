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
    public abstract class ViewBase<TState> : UIBehaviour, IView, IViewTreeElement, ILifetimeScope
        where TState : class, IState
    {
        [NotNull] private readonly ViewRenderScope _renderScope = new ViewRenderScope();
        [NotNull] private readonly List<IViewTreeElement> _children = new List<IViewTreeElement>();
        private readonly LifetimeController _viewLifetimeController = new LifetimeController();

        private readonly MutableAtom<Vector2Int> _bounds = Atom.Value(Vector2Int.zero, debugName: "View.bounds");

        private LifetimeController _stateLifetimeController;

        private TState _currentState;

        private MutableAtom<TState> _nextState;
        private Atom<TState> _doRebind;
        private Atom<object> _doRender;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private CustomSampler _renderSampler;
#endif
        protected bool HasState => _currentState != null;
        protected TState State => _currentState;

        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform => (RectTransform) transform;

        public WidgetViewReference ViewReference { get; set; }

        public bool IsDestroyed { get; private set; }

        public BuildContext Context => State?.Context;

        public Vector2Int Bounds => _bounds.Value;

        public Lifetime ViewLifetime => _viewLifetimeController.Lifetime;

        public Lifetime StateLifetime
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

        Lifetime ILifetimeScope.Lifetime => ViewLifetime;

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

                RefreshBounds();
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

        protected void Unmount()
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
                            OnAfterDeactivate();
                        }
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }

                    DidStateDetached(_currentState);
                }
                
                _stateLifetimeController?.Dispose();
                _stateLifetimeController = null;
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
                    try
                    {
                        Deactivate();
                        OnAfterDeactivate();
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
                    OnAfterActivate();
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
                DidStateAttached(nextState);
            }

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
                    OnAfterRender();
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
            using (Atom.NoWatch)
            {
                var size = rectTransform.rect.size;
                _bounds.Value = new Vector2Int((int) size.x, (int) size.y);
            }
        }

        protected virtual void DidStateAttached(TState state)
        {
        }

        protected virtual void DidStateDetached(TState state)
        {
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

            IsDestroyed = true;
            _viewLifetimeController.Dispose();
        }

        protected virtual void Activate()
        {
        }

        protected virtual void Deactivate()
        {
        }

        protected virtual void OnAfterRender()
        {
        }

        protected virtual void OnAfterActivate()
        {
        }

        protected virtual void OnAfterDeactivate()
        {
        }

        protected abstract void Render();
    }
}