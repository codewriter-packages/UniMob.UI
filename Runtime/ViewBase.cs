using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

namespace UniMob.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ViewBase<TState> : UIBehaviour, IView, IViewTreeElement
        where TState : IState
    {
        [NotNull] private readonly ViewRenderScope _renderScope = new ViewRenderScope();
        [NotNull] private readonly List<IViewTreeElement> _children = new List<IViewTreeElement>();

        private readonly MutableAtom<Vector2> _bounds = Atom.Value(Vector2.zero);

        private bool _mounted;

        private bool _hasState;
        private TState _state;

        private bool _hasSource;
        private readonly MutableAtom<TState> _source;

        private ReactionAtom _renderAtom;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private CustomSampler _renderSampler;
#endif
        protected bool HasState => _hasState;
        protected TState State => _state;

        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform => (RectTransform) transform;

        public WidgetViewReference ViewReference { get; set; }

        public BuildContext Context => State?.Context;

        public Vector2 Bounds => _bounds.Value;

        protected ViewBase()
        {
            var debugName = $"UniMob.ViewBase<{typeof(TState).Name}>::Source";
            _source = Atom.Value(default(TState), debugName: debugName);
        }

        void IView.SetSource(IViewState newSource)
        {
            using (Atom.NoWatch)
            {
                if (ReferenceEquals(_source.Value, newSource))
                {
                    return;
                }
            }

            if (!(newSource is TState nextState))
            {
                var expected = typeof(TState).Name;
                var actual = newSource.GetType().Name;
                Debug.LogError($"Wrong model type at '{name}': expected={expected}, actual={actual}");
                return;
            }

            _renderScope.Link(this);

            if (_renderAtom == null)
            {
                _renderAtom = new ReactionAtom(name, DoRender, OnRenderFailed);
            }

            _hasSource = true;
            _source.Value = nextState;

            if (!_renderAtom.IsActive)
            {
                RefreshBounds();
                _renderAtom.Activate();
            }
        }

        protected void Unmount()
        {
            if (!_hasSource)
            {
                Assert.IsFalse(_hasState, "hasModel");
                Assert.IsFalse(_mounted, "mounted");
                return;
            }

            Assert.IsNotNull(_renderAtom, "renderAtom == null");
            _renderAtom.Deactivate();

            _source.Value = default;
            _hasSource = false;

            if (!_hasState)
            {
                Assert.IsFalse(_mounted, "mounted");
                return;
            }

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

            if (_mounted)
            {
                _mounted = false;
                DidStateDetached(_state);
            }

            foreach (var child in _children)
            {
                child.Unmount();
            }

            _state = default;
            _hasState = false;
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

        private void DoRender()
        {
            Assert.IsTrue(_hasSource, "!hasSource");

            var nextState = _source.Value;
            if (nextState == null)
            {
                Debug.LogWarning("Model == null", this);
                return;
            }

            using (Atom.NoWatch)
            {
                if (!_hasState || !nextState.Equals(_state))
                {
                    if (_hasState)
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

                    _hasState = true;
                    _state = nextState;

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
            }

            Assert.IsNotNull(_renderScope, "renderScope == null");
            using (_renderScope.Enter(this))
            {
                if (isActiveAndEnabled && gameObject.activeSelf)
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
                    if (!_mounted)
                    {
                        _mounted = true;
                        DidStateAttached(_state);
                    }
                }
            }
        }

        private void RefreshBounds()
        {
            _bounds.Value = rectTransform.rect.size;
        }

        protected virtual void DidStateAttached(TState state)
        {
        }

        protected virtual void DidStateDetached(TState state)
        {
        }

        protected virtual void OnRenderFailed(Exception ex)
        {
            Debug.LogException(ex);
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

            if (_hasSource)
            {
                Assert.IsNotNull(_renderAtom, "renderAtom == null");
                RefreshBounds();
                _renderAtom.Actualize();
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