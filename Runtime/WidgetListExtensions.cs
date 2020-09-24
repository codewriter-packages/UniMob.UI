using System.Collections.Generic;

namespace UniMob.UI
{
    public static class WidgetListExtensions
    {
        public static void Add(this List<Widget> list, IEnumerable<Widget> widgets)
        {
            list.AddRange(widgets);
        }
    }
}