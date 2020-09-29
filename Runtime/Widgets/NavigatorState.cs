namespace UniMob.UI.Widgets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using UnityEngine;

    public class NavigatorState : ViewState<Navigator>, INavigatorState
    {
        private readonly StateCollectionHolder _states;
        private readonly NavigatorStack _stack;

        private readonly Queue<NavigatorCommand[]> _pendingCommands = new Queue<NavigatorCommand[]>();
        private readonly Stack<Route> _pendingPause = new Stack<Route>();

        private readonly MutableAtom<bool> _interactable = Atom.Value(true);

        private Task _task = Task.CompletedTask;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Navigator");

        public NavigatorState()
        {
            _stack = new NavigatorStack();
            _states = CreateChildren(_ => _stack.Widgets);
        }

        public bool AutoFocus { get; set; } = true;

        public IState[] Screens => _states.Value;

        public bool Interactable => _interactable.Value;

        public override void InitState()
        {
            base.InitState();

            PushNamed(Widget.InitialRoute);
        }

        private Route CreateRoute(string name)
        {
            if (!Widget.Routes.TryGetValue(name, out var routeBuilder))
            {
                throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown route");
            }

            var route = routeBuilder();

            if (route == null)
            {
                throw new ArgumentException("Route builder result null");
            }

            return route;
        }

        public Task PushNamed(string routeName)
        {
            var route = CreateRoute(routeName);
            return Push(route);
        }

        public Task Push(Route route)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            ApplyCommands(new NavigatorCommand.Push(route));
            return route.PopTask;
        }

        public Task NewRootNamed(string routeName)
        {
            var route = CreateRoute(routeName);
            return NewRoot(route);
        }

        public Task NewRoot(Route route)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            ApplyCommands(
                new NavigatorCommand.PopTo(null),
                new NavigatorCommand.Replace(route));
            return route.PopTask;
        }

        public Task ReplaceNamed(string routeName)
        {
            var route = CreateRoute(routeName);
            return Replace(route);
        }

        public Task Replace(Route route)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            ApplyCommands(new NavigatorCommand.Replace(route));
            return route.PopTask;
        }

        public void PopTo(Route route)
        {
            //if (route == null) throw new ArgumentNullException(nameof(route));

            ApplyCommands(new NavigatorCommand.PopTo(route));
        }

        public void Pop()
        {
            ApplyCommands(new NavigatorCommand.Pop());
        }

        public bool HandleBack()
        {
            return _stack.Count > 1 && _stack.Peek().HandleBack();
        }

        public async Task ApplyScreenEvent(ScreenEvent evt)
        {
            if (_stack.Count == 0)
            {
                return;
            }

            if (!_task.IsCompleted)
            {
                await _task;
            }

            switch (evt)
            {
                case ScreenEvent.Create:
                    break;

                case ScreenEvent.Resume:
                    await UnpauseScreens();
                    break;

                case ScreenEvent.Focus:
                    await _stack.Peek().ApplyScreenEvent(ScreenEvent.Focus);
                    break;

                case ScreenEvent.Unfocus:
                    await _stack.Peek().ApplyScreenEvent(ScreenEvent.Unfocus);
                    break;

                case ScreenEvent.Pause:
                    await PauseScreens();
                    break;

                case ScreenEvent.Destroy:
                    while (_stack.Count > 0)
                    {
                        await _stack.Peek().ApplyScreenEvent(ScreenEvent.Destroy);
                        _stack.Pop();
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, "Unexpected event");
            }
        }

        private void ApplyCommands([NotNull] params NavigatorCommand[] commands)
        {
            _pendingCommands.Enqueue(commands);

            if (_task.IsCompleted)
            {
                _task = ProcessCommandsLoop();
            }
        }

        private async Task ProcessCommandsLoop()
        {
            _interactable.Value = false;
            try
            {
                while (_pendingCommands.Count > 0)
                {
                    await ProcessCommands(_pendingCommands.Dequeue());
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _interactable.Value = true;
            }
        }

        private async Task ProcessCommands([NotNull] NavigatorCommand[] commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            foreach (var command in commands)
            {
                await ProcessCommand(command);
            }

            if (AutoFocus && _stack.Count > 0)
            {
                await _stack.Peek().ApplyScreenEvent(ScreenEvent.Focus);
            }
        }

        private Task ProcessCommand([NotNull] NavigatorCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            switch (command)
            {
                case NavigatorCommand.Pop _:
                    return PopInternal();

                case NavigatorCommand.PopTo backTo:
                    return PopToInternal(backTo);

                case NavigatorCommand.Push forward:
                    return PushInternal(forward);

                case NavigatorCommand.Replace replace:
                    return ReplaceInternal(replace);
            }

            Debug.LogWarning($"Unexpected navigator command: {command.GetType().Name}");
            return Task.CompletedTask;
        }

        private async Task PushInternal(NavigatorCommand.Push push)
        {
            if (push.Route.ModalType == RouteModalType.Fullscreen)
            {
                await PauseScreens();
            }
            else if (_stack.Count >= 1 && _stack.Peek().ScreenState == ScreenState.Focused)
            {
                await _stack.Peek().ApplyScreenEvent(ScreenEvent.Unfocus);
            }

            var screen = push.Route;
            await InitializeScreen(push.Route);

            _stack.Push(screen);
            await screen.ApplyScreenEvent(ScreenEvent.Create);
        }

        private async Task ReplaceInternal(NavigatorCommand.Replace replace)
        {
            if (_stack.Count > 0)
            {
                await _stack.Peek().ApplyScreenEvent(ScreenEvent.Destroy);
                _stack.Pop();

                if (_stack.Count > 0)
                {
                    var isNewFullscreen = replace.Route.ModalType == RouteModalType.Fullscreen;
                    var isOldFullscreen = _stack.Peek().ModalType == RouteModalType.Fullscreen;

                    if (isNewFullscreen && !isOldFullscreen)
                    {
                        await PauseScreens();
                    }
                    else if (!isNewFullscreen && isOldFullscreen)
                    {
                        await UnpauseScreens();
                    }
                }
            }

            var screen = replace.Route;
            await InitializeScreen(screen);

            _stack.Push(screen);
            await screen.ApplyScreenEvent(ScreenEvent.Create);
        }

        private async Task PopToInternal(NavigatorCommand.PopTo popTo)
        {
            while (_stack.Count > 1)
            {
                if (popTo.Route != null && _stack.Peek().Key == popTo.Route.Key)
                {
                    break;
                }

                var first = _stack.Peek();
                var destroyTask = first.ApplyScreenEvent(ScreenEvent.Destroy);

                if (first.ModalType == RouteModalType.Fullscreen)
                {
                    await UnpauseScreens(skip: 1);
                }

                await destroyTask;
                _stack.Pop();
            }
        }

        private async Task PopInternal()
        {
            if (_stack.Count <= 1)
            {
                return;
            }

            var first = _stack.Peek();
            var destroyTask = first.ApplyScreenEvent(ScreenEvent.Destroy);

            if (first.ModalType == RouteModalType.Fullscreen)
            {
                await UnpauseScreens(skip: 1);
            }

            await destroyTask;
            _stack.Pop();
        }

        private async Task PauseScreens()
        {
            foreach (var screen in _stack)
            {
                _pendingPause.Push(screen);

                if (screen.ModalType == RouteModalType.Fullscreen)
                {
                    break;
                }
            }

            while (_pendingPause.Count > 0)
            {
                var screen = _pendingPause.Pop();
                await screen.ApplyScreenEvent(ScreenEvent.Pause);
            }
        }

        private async Task UnpauseScreens(int skip = 0)
        {
            foreach (var screen in _stack)
            {
                if (skip-- != 0)
                {
                    continue;
                }

                await screen.ApplyScreenEvent(ScreenEvent.Resume);

                if (screen.ModalType == RouteModalType.Fullscreen)
                {
                    return;
                }
            }
        }

        private Task InitializeScreen(Route screen)
        {
            screen.NavigatorState = this;
            return screen.Initialize();
        }
    }

    internal abstract class NavigatorCommand
    {
        public sealed class Pop : NavigatorCommand
        {
        }

        public sealed class PopTo : NavigatorCommand
        {
            public Route Route { get; }

            public PopTo([CanBeNull] Route route) => Route = route;
        }

        public class Push : NavigatorCommand
        {
            public Route Route { get; }

            public Push([NotNull] Route route) => Route = route;
        }

        public class Replace : NavigatorCommand
        {
            public Route Route { get; }

            public Replace([NotNull] Route route) => Route = route;
        }
    }

    internal class NavigatorStack : IEnumerable<Route>
    {
        private readonly Stack<Route> _stack = new Stack<Route>();
        private readonly List<Widget> _widgets = new List<Widget>();
        private readonly MutableAtom<int> _version = Atom.Value(int.MinValue);

        public int Count => _stack.Count;

        public List<Widget> Widgets
        {
            get
            {
                _version.Get();
                return _widgets;
            }
        }

        public Route Peek() => _stack.Peek();

        public Route Pop()
        {
            _widgets.RemoveAt(_widgets.Count - 1);
            var result = _stack.Pop();
            _version.Value++;
            return result;
        }

        public void Push(Route screen)
        {
            _widgets.Add(new Builder(screen.Build)
            {
                Key = Key.Of(screen)
            });
            _stack.Push(screen);
            _version.Value++;
        }

        public IEnumerator<Route> GetEnumerator() => _stack.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}