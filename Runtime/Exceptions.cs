using System;

namespace UniMob.UI
{
    public sealed class WrongStateTypeException : Exception
    {
        public WrongStateTypeException(Type stateType, Type expectedWidgetType, Type actualWidgetType)
            : base($"Trying to pass {actualWidgetType} widget to state {stateType}, but expected {expectedWidgetType}")
        {
        }
    }
}