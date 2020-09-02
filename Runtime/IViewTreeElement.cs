namespace UniMob.UI
{
    public interface IViewTreeElement
    {
        void AddChild(IViewTreeElement view);
        void Unmount();
    }
}