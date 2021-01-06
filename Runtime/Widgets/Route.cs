using UniMob.UI.Internal;

namespace UniMob.UI.Widgets
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;

    public abstract class Route
    {
        private readonly RouteSettings _settings;
        private readonly TriggerStateMachine<ScreenState, ScreenEvent, Task> _machine;
        private readonly TaskCompletionSource<object> _popCompleter = new TaskCompletionSource<object>();

        private object _popResult = null;

        internal NavigatorState NavigatorState { get; set; }

        protected Route(RouteSettings settings)
        {
            _settings = settings;
            _machine = BuildStateMachine();
        }

        public ScreenState ScreenState => _machine.State;

        public RouteModalType ModalType => _settings.ModalType;

        public Task<object> PopTask => _popCompleter.Task;

        public string Key => _settings.Name;

        private TriggerStateMachine<ScreenState, ScreenEvent, Task> BuildStateMachine()
        {
            // Initializing → Created → Destroyed
            //                  ↓ ↑
            //                Resumed
            //                  ↓ ↑
            //                Focused
            var fsm = new TriggerStateMachine<ScreenState, ScreenEvent, Task>(ScreenState.Initializing);

            fsm.On(ScreenEvent.Create)
                .Allow(ScreenState.Initializing, ScreenState.Created, ExecTransition(OnCreate))
                .Allow(ScreenState.Created, ScreenState.Created);

            fsm.On(ScreenEvent.Resume)
                .Allow(ScreenState.Created, ScreenState.Resumed, ExecTransition(OnResume, ScreenEvent.Resume))
                .Allow(ScreenState.Resumed, ScreenState.Resumed);

            fsm.On(ScreenEvent.Focus)
                .Allow(ScreenState.Created, ScreenState.Resumed, ExecTransition(OnResume, ScreenEvent.Focus))
                .Allow(ScreenState.Resumed, ScreenState.Focused, ExecTransition(OnFocus))
                .Allow(ScreenState.Focused, ScreenState.Focused);

            fsm.On(ScreenEvent.Unfocus)
                .Allow(ScreenState.Focused, ScreenState.Resumed, ExecTransition(OnFocusLost))
                .Allow(ScreenState.Resumed, ScreenState.Resumed);

            fsm.On(ScreenEvent.Pause)
                .Allow(ScreenState.Focused, ScreenState.Resumed, ExecTransition(OnFocusLost, ScreenEvent.Pause))
                .Allow(ScreenState.Resumed, ScreenState.Created, ExecTransition(OnPause))
                .Allow(ScreenState.Created, ScreenState.Created);

            fsm.On(ScreenEvent.Destroy)
                .Allow(ScreenState.Focused, ScreenState.Resumed, ExecTransition(OnFocusLost, ScreenEvent.Destroy))
                .Allow(ScreenState.Resumed, ScreenState.Created, ExecTransition(OnPause, ScreenEvent.Destroy))
                .Allow(ScreenState.Created, ScreenState.Destroyed, ExecTransition(OnDestroy))
                .Allow(ScreenState.Destroyed, ScreenState.Destroyed);

            return fsm;
        }

        private Func<Task, Task> ExecTransition(Func<Task> handler, ScreenEvent? screenEvent = null) =>
            previous => ExecuteTransitionInternal(previous, handler, screenEvent);

        private async Task ExecuteTransitionInternal(Task previous, Func<Task> handler, ScreenEvent? screenEvent)
        {
            if (previous != null && !previous.IsCompleted)
            {
                await previous;
            }

            var current = handler();

            if (current != null && !current.IsCompleted)
            {
                await current;
            }

            if (screenEvent.HasValue)
            {
                var next = _machine.Trigger(screenEvent.Value);

                if (next != null && !next.IsCompleted)
                {
                    await next;
                }
            }
        }

        public virtual Task ApplyScreenEvent(ScreenEvent screenEvent)
        {
            if (_machine.CanTrigger(screenEvent))
            {
                return _machine.Trigger(screenEvent) ?? Task.CompletedTask;
            }

            Debug.LogErrorFormat("Cannot {0} scene {1} in {2} state", screenEvent, GetType().Name, ScreenState);
            return Task.CompletedTask;
        }

        public Task Initialize()
        {
            return OnInitialize() ?? Task.CompletedTask;
        }

        protected virtual Task OnInitialize() => Task.CompletedTask;

        protected virtual Task OnCreate() => Task.CompletedTask;

        protected virtual Task OnPause() => Task.CompletedTask;

        protected virtual Task OnResume() => Task.CompletedTask;

        protected virtual Task OnFocus() => Task.CompletedTask;

        protected virtual Task OnFocusLost() => Task.CompletedTask;

        protected virtual Task OnDestroy()
        {
            _popCompleter.SetResult(this._popResult);
            return Task.CompletedTask;
        }

        public virtual bool HandleBack() => true;

        public abstract Widget Build(BuildContext context);

        public void SetResult(object result) {

            _popResult = result;
        }
    }

    public enum ScreenEvent
    {
        Create = 0,
        Resume = 1,
        Focus = 2,
        Unfocus = 3,
        Pause = 4,
        Destroy = 5,
    }

    public enum ScreenState
    {
        Initializing = 0,
        Created = 1,
        Resumed = 2,
        Focused = 3,
        Destroyed = 4,
    }

    public enum RouteModalType
    {
        Fullscreen,
        Popup,
    }

    public class RouteSettings
    {
        public string Name { get; }

        public RouteModalType ModalType { get; }

        public RouteSettings(string name, RouteModalType modalType)
        {
            Name = name;
            ModalType = modalType;
        }
    }
}