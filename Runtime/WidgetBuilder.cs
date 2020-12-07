using JetBrains.Annotations;

namespace UniMob.UI
{
    public delegate TWidget WidgetBuilder<out TWidget>([NotNull] BuildContext context)
        where TWidget : Widget;
}