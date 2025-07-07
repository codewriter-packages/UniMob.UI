using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMob.UI
{
    public class StateProviderWidget : ProxyWidget
    {
        public IStateProvider StateProvider { get; }
        
        public StateProviderWidget(Func<Widget> childBuilder, IStateProvider stateProvider) : base(childBuilder)
        {
            StateProvider = stateProvider;
        }

        public StateProviderWidget(Widget child, IStateProvider stateProvider) : base(child)
        {
            StateProvider = stateProvider;
        }

        public override State CreateState() => new StateProviderState();

    }

    public class StateProviderState : ProxyState<StateProviderWidget>, IStateProviderSource
    {
        public IStateProvider StateProvider => Widget.StateProvider;
    }
}
