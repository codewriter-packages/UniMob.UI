using System;
using JetBrains.Annotations;
using UniMob.UI.Internal;
using UniMob.UI.Widgets;

namespace UniMob.UI
{
    public static class UniMobUI
    {
        public static IDisposable RunApp([NotNull] ViewPanel root, [NotNull] WidgetBuilder<Widget> builder,
            string debugName = null)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            IView view = root;
            var context = new BuildContext(null, null);
            var stateHolder = State.Create<Widget, IState>(context, builder);
            var render = Atom.Reaction(() => root.Render(stateHolder.Value), debugName: debugName);

            // ReSharper disable once ImplicitlyCapturedClosure
            return new ActionDisposable(() =>
            {
                render.Deactivate();

                if (!Engine.IsApplicationQuiting)
                {
                    view.ResetSource();
                }
            });
        }

        private class ActionDisposable : IDisposable
        {
            private readonly Action _action;
            public ActionDisposable(Action action) => _action = action;
            public void Dispose() => _action?.Invoke();
        }
    }
}