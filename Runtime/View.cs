using System;

namespace UniMob.UI
{
    public abstract class View<TState> : ViewBase<TState>
        where TState : class, IViewState
    {
        public void Render(TState state, bool link = false)
        {
            var self = (IView) this;
            self.SetSource(state, link);
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