using System;
using JetBrains.Annotations;
using UniMob.UI.Layout;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI
{
    public static class UniMobUI
    {
        public static void RunApp(Lifetime lifetime, StateProvider stateProvider,
            [NotNull] ViewPanel root, [NotNull] WidgetBuilder<Widget> builder,
            string debugName = null)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var rootContext = new BuildContext(null, null);
          
            
            var stateHolder = State.Create<Widget, IState>(lifetime, rootContext, ctx =>
            {
                var child = builder.Invoke(ctx);
                return new UniMobDeviceWidget(child, root.gameObject, stateProvider);
            });

            IView view = root;
            lifetime.Register(() => view.ResetSource());

            Atom.Reaction(lifetime, () => root.Render(stateHolder.Value), debugName: debugName);
        }
    }
}