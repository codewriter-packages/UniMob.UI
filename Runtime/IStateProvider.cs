using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMob.UI
{
    public interface IStateProvider
    {
        State Of(Widget widget);
    }
}
