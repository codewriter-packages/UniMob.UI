using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    /// <summary>
    ///     The base class for all new widgets that participate in the multipass layout system.
    /// </summary>
    public abstract class LayoutWidget : StatefulWidget
    {
        /// <summary>
        ///     Creates the lightweight RenderObject responsible for layout calculations.
        /// </summary>
        public abstract RenderObject CreateRenderObject(BuildContext context, ILayoutState state);
    }
}