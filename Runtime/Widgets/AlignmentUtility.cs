using UnityEngine;

namespace UniMob.UI.Widgets
{
    public static class AlignmentUtility
    {
        public static Alignment ToAlignment(MainAxisAlignment mainAxis, CrossAxisAlignment crossAxis)
        {
            var alignX = crossAxis == CrossAxisAlignment.Start ? Alignment.TopLeft.X
                : crossAxis == CrossAxisAlignment.End ? Alignment.TopRight.X
                : Alignment.Center.X;

            var alignY = mainAxis == MainAxisAlignment.Start ? Alignment.TopCenter.Y
                : mainAxis == MainAxisAlignment.End ? Alignment.BottomCenter.Y
                : Alignment.Center.Y;

            return new Alignment(alignX, alignY);
        }

        public static Vector3 ToOffset(MainAxisAlignment mainAxis, CrossAxisAlignment crossAxis)
        {
            var offsetMultiplierX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                : crossAxis == CrossAxisAlignment.End ? 1.0f
                : 0.5f;

            var offsetMultiplierY = mainAxis == MainAxisAlignment.Start ? 0.0f
                : mainAxis == MainAxisAlignment.End ? 1.0f
                : 0.5f;

            return new Vector3(offsetMultiplierX, offsetMultiplierY);
        }
    }
}