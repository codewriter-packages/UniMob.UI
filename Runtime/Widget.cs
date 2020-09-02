using System;

namespace UniMob.UI
{
    // ReSharper disable once InconsistentNaming
    public interface Widget
    {
        Type Type { get; }

        Key Key { get; }

        State CreateState();
    }
}