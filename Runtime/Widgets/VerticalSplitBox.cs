namespace UniMob.UI.Widgets
{
    public class VerticalSplitBox : StatefulWidget
    {
        public Widget FirstChild { get; set; }
        public Widget SecondChild { get; set; }

        public override State CreateState() => new VerticalSplitBoxState();
    }

    public class VerticalSplitBoxState : ViewState<VerticalSplitBox>, IVerticalSplitBoxState
    {
        private StateHolder _firstChild;
        private StateHolder _secondChild;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_VerticalSplitBox");

        public IState FirstChild => _firstChild.Value;
        public IState SecondChild => _secondChild.Value;

        public override void InitState()
        {
            base.InitState();

            _firstChild = CreateChild(_ => Widget.FirstChild);
            _secondChild = CreateChild(_ => Widget.SecondChild);
        }

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.StackY(FirstChild.Size, SecondChild.Size);
        }
    }
}