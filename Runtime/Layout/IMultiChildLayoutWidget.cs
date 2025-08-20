using System.Collections.Generic;

namespace UniMob.UI.Layout
{
    public interface IMultiChildLayoutWidget
    {
        List<Widget> Children { get; }
    }
}