using System;
using System.Collections.Generic;

namespace UniMob.UI
{
    public static class WidgetListExtensions
    {
        public static void Add(this List<Widget> list, IEnumerable<Widget> widgets)
        {
            list.AddRange(widgets);
        }

        public static void Add(this List<Widget> list, (bool when, Func<Widget> add) value)
        {
            if (value.when)
            {
                list.Add(value.add.Invoke());
            }
        }

        public static void Add(this List<Widget> list, (bool when, Func<IEnumerable<Widget>> add) value)
        {
            if (value.when)
            {
                list.AddRange(value.add.Invoke());
            }
        }
    }
}