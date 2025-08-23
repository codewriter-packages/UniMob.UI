using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// The base class for all new widgets that participate in the multipass layout system.
    /// </summary>
    public abstract class LayoutWidget : StatefulWidget
    {
        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return CreateRenderObject(context, (ILayoutState) state);
        }

        public abstract RenderObject CreateRenderObject(BuildContext context, ILayoutState state);
    }
}