using System;
using System.Collections.Generic;
using System.Linq;
using UniMob.UI.Internal;

namespace UniMob.UI
{
    public abstract class State : IState, IDisposable, ILifetimeScope
    {
        private readonly MutableBuildContext _context;

        private LifetimeController _stateLifetimeController;

        public BuildContext Context => _context;

        internal Widget RawWidget { get; private set; }

        public abstract IViewState InnerViewState { get; }

        public abstract WidgetSize Size { get; }

        public Key Key => RawWidget.Key;

        Lifetime ILifetimeScope.Lifetime => StateLifetime;

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

        protected State()
        {
            _context = new MutableBuildContext(this, null);
        }

        internal virtual void Update(Widget widget)
        {
            RawWidget = widget;
        }

        internal void Mount(BuildContext context)
        {
            if (Context.Parent != null)
                throw new InvalidOperationException("State already mounted");

            _context.SetParent(context);
        }

        public virtual void InitState()
        {
        }

        public virtual void Dispose()
        {
            _stateLifetimeController?.Dispose();
        }

        internal static StateHolder<TState> Create<TWidget, TState>(
            Lifetime lifetime,
            BuildContext context,
            WidgetBuilder<TWidget> builder)
            where TWidget : Widget
            where TState : class, IState
        {
            return new StateHolder<TWidget, TState>(lifetime, context, builder);
        }

        internal static StateCollectionHolder CreateList(
            Lifetime lifetime,
            BuildContext context,
            Func<BuildContext,
                List<Widget>> builder)
        {
            return new StateCollectionHolder(lifetime, context, builder);
        }
    }

    public class StateCollectionHolder
    {
        private readonly BuildContext _context;
        private readonly Func<BuildContext, List<Widget>> _builder;
        private readonly Atom<IState[]> _statesAtom;

        private State[] _states = new State[0];

        public IState[] Value => _statesAtom.Value;

        public StateCollectionHolder(Lifetime lifetime, BuildContext context, Func<BuildContext, List<Widget>> builder)
        {
            _context = context;
            _builder = builder;
            _statesAtom = Atom.Computed(lifetime, ComputeStates);

            lifetime.Register(DeactivateStates);
        }

        private State[] ComputeStates()
        {
            var newWidgets = _builder.Invoke(_context);
            using (Atom.NoWatch)
            {
                _states = StateUtilities.UpdateChildren(_context, _states, newWidgets);
            }

            return _states.ToArray();
        }

        private void DeactivateStates()
        {
            foreach (var state in _states)
            {
                StateUtilities.DeactivateChild(state);
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    public interface StateHolder
    {
        IState Value { get; }
    }

    // ReSharper disable once InconsistentNaming
    public interface StateHolder<out TState> : StateHolder
    {
        new TState Value { get; }
    }

    public sealed class StateHolder<TWidget, TState> : StateHolder<TState>
        where TWidget : Widget
        where TState : class, IState
    {
        private readonly BuildContext _context;
        private readonly WidgetBuilder<TWidget> _builder;
        private readonly Atom<TState> _stateAtom;

        private State _state;

        public StateHolder(Lifetime lifetime, BuildContext context, WidgetBuilder<TWidget> builder)
        {
            _context = context;
            _builder = builder;
            _stateAtom = Atom.Computed(lifetime, ComputeState);
            lifetime.Register(DeactivateState);
        }

        IState StateHolder.Value => _stateAtom.Value;
        TState StateHolder<TState>.Value => _stateAtom.Value;

        private TState ComputeState()
        {
            var newWidget = _builder(_context);
            using (Atom.NoWatch)
            {
                _state = StateUtilities.UpdateChild(_context, _state, newWidget);
            }

            return _state as TState;
        }

        private void DeactivateState()
        {
            if (_state != null)
            {
                StateUtilities.DeactivateChild(_state);
            }
        }
    }
}