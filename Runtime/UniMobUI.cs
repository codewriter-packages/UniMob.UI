using System;
using JetBrains.Annotations;
using UniMob.UI.Widgets;

namespace UniMob.UI
{
    public static class UniMobUI
    {
        public static IDisposable RunApp(string debugName, [NotNull] ViewPanel root, [NotNull] WidgetBuilder builder)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            IView view = root;
            var context = new BuildContext(null, null);
            var state = State.Create(context, builder);
            var render = Atom.Reaction(debugName, () => root.Render(state.Value));

            // ReSharper disable once ImplicitlyCapturedClosure
            return new ActionDisposable(() =>
            {
                render.Dispose();

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