using UniMob.UI.Layout;

namespace UniMob.UI
{
    public static class StateExtensions
    {
        /// <summary>
        ///     Robustly checks if a state, or any of its inner wrapped states, is an ILayoutState.
        ///     This correctly handles proxy widgets like HocState.
        /// </summary>
        /// <param name="state">The state to check.</param>
        /// <param name="layoutState">The unwrapped layout state, if found.</param>
        /// <returns>True if the underlying state is an ILayoutState.</returns>
        public static bool AsLayoutState(this IState state, out ILayoutState layoutState)
        {
            if (state?.InnerViewState is ILayoutState innerLayoutState)
            {
                layoutState = innerLayoutState;
                return true;
            }

            layoutState = null;
            return false;
        }
    }
}