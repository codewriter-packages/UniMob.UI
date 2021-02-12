using System;

namespace UniMob.UI
{
    public abstract class View<TState> : ViewBase<TState>
        where TState : class, IViewState
    {
        public void Render(TState state)
        {
            var self = (IView) this;
            self.SetSource(state);
        }

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