using System;
using JetBrains.Annotations;
using UniMob.UI.Widgets;

namespace UniMob.UI
{
    public static class UniMobUI
    {
        public static void RunApp(Lifetime lifetime, [NotNull] ViewPanel root, [NotNull] WidgetBuilder<Widget> builder,
            string debugName = null)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            IView view = root;
            var context = new BuildContext(null, null);
            var stateHolder = State.Create<Widget, IState>(lifetime, context, builder);

            lifetime.Register(() => view.ResetSource());

            Atom.Reaction(lifetime, () => root.Render(stateHolder.Value), debugName: debugName);
        }
    }
}