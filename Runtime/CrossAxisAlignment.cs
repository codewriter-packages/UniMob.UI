namespace UniMob.UI
{
    public enum CrossAxisAlignment
    {
        /// <summary>
        /// Place the children with their start edge aligned with the start side of the cross axis.
        /// </summary>
        Start = 0,

        /// <summary>
        /// Place the children as close to the end of the cross axis as possible.
        /// </summary>
        End = 1,

        /// <summary>
        /// Place the children so that their centers align with the middle of the cross axis.
        /// </summary>
        Center = 2,
        /*
        /// <summary>
        /// This causes the constraints passed to the children to be tight in the cross axis.
        /// </summary>
        Stretch = 3,
        */
    }
}