namespace UniMob.UI.Layout
{
    public interface ISingleChildLayoutState : ILayoutState
    {
        IState Child { get; }
    }
}