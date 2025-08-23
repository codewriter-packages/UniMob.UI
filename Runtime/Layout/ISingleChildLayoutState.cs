namespace UniMob.UI.Layout
{
    public interface ISingleChildLayoutState : IState
    {
        IState Child { get; }
    }
}