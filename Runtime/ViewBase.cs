using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

namespace UniMob.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ViewBase<TState> : UIBehaviour, IView, IViewTreeElement
        where TState : class, IState
    {
        [NotNull] private readonly ViewRenderScope _renderScope = new ViewRenderScope();
        [NotNull] private readonly List<IViewTreeElement> _children = new List<IViewTreeElement>();

        private readonly MutableAtom<Vector2Int> _bounds = Atom.Value(Vector2Int.zero);

        private TState _currentState;

        private readonly MutableAtom<object> _nextStateRaw;
        private readonly Atom<TState> _nextStateTyped;
        private readonly Atom<object> _doRebind;
        private readonly Atom<object> _doRender;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private CustomSampler _renderSampler;
#endif
        protected bool HasState => _currentState != null;
        protected TState State => _currentState;

        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform => (RectTransform) transform;

        public WidgetViewReference ViewReference { get; set; }

        public BuildContext Context => State?.Context;

        public Vector2Int Bounds => _bounds.Value;

        protected ViewBase()
        {
            _nextStateRaw = Atom.Value<object>(null, debugName: "ViewBase.nextStateRaw");
            _nextStateTyped = Atom.Computed(GetNextStateTyped, debugName: "ViewBase.nextStateTyped");
            _doRebind = Atom.Computed(DoRebind, debugName: "ViwBase.DoRebind()", keepAlive: true);
            _doRender = Atom.Computed(DoRender, debugName: "ViewBase.DoRender()", keepAlive: true);
        }

        private TState GetNextStateTyped()
        {
            var state = _nextStateRaw.Value;
            if (state is TState typedState)
            {
                return typedState;
            }

            var expected = typeof(TState).Name;
            var actual = state.GetType().Name;
            Debug.LogError($"Wrong model type at '{name}': expected={expected}, actual={actual}");
            return null;
        }

        void IView.SetSource(IViewState newSource, bool link)
        {
            _renderScope.Link(this);

            if (!ReferenceEquals(newSource, _currentState))
            {
                _nextStateRaw.Value = newSource;

                RefreshBounds();
            }

            if (link)
            {
                _doRebind.Get();
            }
            else
            {
                ((AtomBase) _doRebind).Actualize();
            }
        }

        protected void Unmount()
        {
            using (Atom.NoWatch)
            {
                _doRebind.Deactivate();
                _doRender.Deactivate();

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
            }

            foreach (var child in _children)
            {
                child.Unmount();
            }

            _nextStateRaw.Value = null;
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

        private object DoRebind()
        {
            var nextState = _nextStateTyped.Value;

            using (Atom.NoWatch)
            {
                var currentState = _currentState;
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

            return null;
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

                try
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    _renderSampler.Begin();
#endif
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
            var size = rectTransform.rect.size;
            _bounds.Value = new Vector2Int((int) size.x, (int) size.y);
        }

        protected virtual void DidStateAttached(TState state)
        {
        }

        protected virtual void DidStateDetached(TState state)
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_renderSampler == null || !_renderSampler.name.EndsWith(name))
            {
                _renderSampler = CustomSampler.Create($"Render {name}");
            }
#endif

            RefreshBounds();

            ((AtomBase) _doRebind).Actualize();
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