using System;

namespace UniMob.UI
{
    public abstract class View<TState> : ViewBase<TState>
        where TState : IViewState
    {
        protected sealed override void DidStateAttached(TState state)
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

        protected sealed override void DidStateDetached(TState state)
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
    }
}