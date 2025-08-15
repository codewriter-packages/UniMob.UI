namespace UniMob.UI.Layout
{
    public enum FlexMainAxisAlignment
    {
        /// <summary>
        /// Place the children as close to the start of the main axis as possible.
        /// </summary>
        Start,
        /// <summary>
        /// Place the children as close to the end of the main axis as possible.
        /// </summary>
        End,
        /// <summary>
        /// Place the children as close to the middle of the main axis as possible.
        /// </summary>
        Center,
        
        /// <summary>
        /// Place the free space evenly between the children. Only supported in Layout widgets.
        /// </summary>
        SpaceBetween,
        
        /// <summary>
        /// Place the free space evenly between the children as well as half
        /// of that space before and after the first and last child.
        /// </summary>
        /// <remarks>Only supported in Layout widgets.</remarks>
        SpaceAround,
        
        /// <summary>
        /// Place the free space evenly between the children as well as before and after the first and last child.
        /// </summary>
        SpaceEvenly
    }
}