using System;
using System.Collections.Generic;
using System.Linq;
using UniMob.UI.Internal;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class State : IState, IDisposable
    {
        private readonly MutableBuildContext _context;

        public BuildContext Context => _context;

        internal Widget RawWidget { get; private set; }

        public abstract IViewState InnerViewState { get; }

        public abstract WidgetSize Size { get; }

        protected State()
        {
            Assert.IsNull(Atom.CurrentScope);
            _context = new MutableBuildContext(this, null);
        }

        internal virtual void Update(Widget widget)
        {
            RawWidget = widget;
        }

        internal void Mount(BuildContext context)
        {
            Assert.IsNull(Atom.CurrentScope);

            if (Context.Parent != null)
                throw new InvalidOperationException();

            _context.SetParent(context);
        }

        public virtual void InitState()
        {
            Assert.IsNull(Atom.CurrentScope);
        }

        public virtual void Dispose()
        {
            Assert.IsNull(Atom.CurrentScope);
        }

        internal static Atom<IState> Create(BuildContext context, WidgetBuilder builder)
        {
            Assert.IsNull(Atom.CurrentScope);

            State state = null;
            return Atom.Computed<IState>(() =>
            {
                var newWidget = builder(context);
                using (Atom.NoWatch)
                {
                    state = StateUtilities.UpdateChild(context, state, newWidget);
                }

                return state;
            }, callbacks: new ActionAtomCallbacks(
                onActive: () => { },
                onInactive: () => StateUtilities.DeactivateChild(state)
            ), requiresReaction: true);
        }

        internal static Atom<IState[]> CreateList(BuildContext context, Func<BuildContext, List<Widget>> builder)
        {
            Assert.IsNull(Atom.CurrentScope);

            var states = new State[0];
            return Atom.Computed<IState[]>(() =>
            {
                var newWidgets = builder.Invoke(context);
                using (Atom.NoWatch)
                {
                    states = StateUtilities.UpdateChildren(context, states, newWidgets);
                }

                // ReSharper disable once CoVariantArrayConversion
                return states.ToArray();
            }, callbacks: new ActionAtomCallbacks(
                onActive: () => { },
                onInactive: () =>
                {
                    foreach (var state in states)
                    {
                        StateUtilities.DeactivateChild(state);
                    }
                }
            ), requiresReaction: true);
        }
    }
}