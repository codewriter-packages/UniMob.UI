using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniMob.UI.Internal;

namespace UniMob.UI
{
    public abstract class State : IState, IDisposable
    {
        private readonly MutableBuildContext _context;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public BuildContext Context => _context;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        internal Widget RawWidget { get; private set; }

        public abstract IViewState InnerViewState { get; }

        public abstract WidgetSize Size { get; }

        protected State()
        {
            _context = new MutableBuildContext(this, null);
            _cancellationTokenSource = new CancellationTokenSource();
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
            _cancellationTokenSource.Dispose();
        }

        internal static StateHolder Create(BuildContext context, WidgetBuilder builder)
        {
            return new StateHolder(context, builder);
        }

        internal static StateCollectionHolder CreateList(BuildContext context, Func<BuildContext, List<Widget>> builder)
        {
            return new StateCollectionHolder(context, builder);
        }
    }

    public class StateCollectionHolder : IAtomCallbacks
    {
        private readonly BuildContext _context;
        private readonly Func<BuildContext, List<Widget>> _builder;
        private readonly Atom<IState[]> _statesAtom;

        private State[] _states = new State[0];

        public IState[] Value => _statesAtom.Value;

        public StateCollectionHolder(BuildContext context, Func<BuildContext, List<Widget>> builder)
        {
            _context = context;
            _builder = builder;
            _statesAtom = Atom.Computed(ComputeStates, callbacks: this, requiresReaction: true);
        }

        private State[] ComputeStates()
        {
            var newWidgets = _builder.Invoke(_context);
            using (Atom.NoWatch)
            {
                _states = StateUtilities.UpdateChildren(_context, _states, newWidgets);
            }

            // ReSharper disable once CoVariantArrayConversion
            return _states.ToArray();
        }

        void IAtomCallbacks.OnActive()
        {
        }

        void IAtomCallbacks.OnInactive()
        {
            foreach (var state in _states)
            {
                StateUtilities.DeactivateChild(state);
            }
        }
    }

    public class StateHolder : IAtomCallbacks
    {
        private readonly BuildContext _context;
        private readonly WidgetBuilder _builder;
        private readonly Atom<IState> _stateAtom;

        private State _state;

        public StateHolder(BuildContext context, WidgetBuilder builder)
        {
            _context = context;
            _builder = builder;
            _stateAtom = Atom.Computed(ComputeState, callbacks: this, requiresReaction: true);
        }

        public IState Value => _stateAtom.Value;

        private IState ComputeState()
        {
            var newWidget = _builder(_context);
            using (Atom.NoWatch)
            {
                _state = StateUtilities.UpdateChild(_context, _state, newWidget);
            }

            return _state;
        }

        void IAtomCallbacks.OnActive()
        {
        }

        void IAtomCallbacks.OnInactive()
        {
            StateUtilities.DeactivateChild(_state);
        }
    }
}