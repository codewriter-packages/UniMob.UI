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
        
        public StateProviderWidget(IStateProvider stateProvider, Func<Widget> childBuilder) : base(childBuilder)
        {
            StateProvider = stateProvider;
        }

        public StateProviderWidget(IStateProvider stateProvider, Widget child) : base(child)
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
