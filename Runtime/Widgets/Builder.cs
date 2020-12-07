using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class Builder : StatefulWidget
    {
        public Builder(
            [NotNull] WidgetBuilder<Widget> build
        )
        {
            Build = build;
        }

        public WidgetBuilder<Widget> Build { get; }

        public override State CreateState() => new BuilderState();
    }

    internal class BuilderState : HocState<Builder>
    {
        public override Widget Build(BuildContext context) => Widget.Build.Invoke(context);
    }
}