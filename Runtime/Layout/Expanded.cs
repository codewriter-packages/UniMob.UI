namespace UniMob.UI.Layout
{
    /// <summary>
    /// A widget that expands a child of a Row or Column to fill the available space.
    /// This is a signal widget and does not create its own RenderObject.
    /// </summary>
    public class Expanded : StatefulWidget
    {
        public int Flex { get; set; } = 1;
        public Widget Child { get; set; }

        public override State CreateState() => new ExpandedState();
    }

    public class ExpandedState : HocState<Expanded>
    {
        public override Widget Build(BuildContext context)
        {
            return Widget.Child;
        }
    }
}