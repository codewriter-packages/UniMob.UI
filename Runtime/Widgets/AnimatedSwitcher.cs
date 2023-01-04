using System.Linq;
using System.Collections.Generic;
using UniMob.UI.Internal;
using UnityEngine.Assertions;

namespace UniMob.UI.Widgets
{
    public class AnimatedSwitcher : StatefulWidget
    {
        public Widget Child { get; set; }
        public float Duration { get; set; }
        public float ReverseDuration { get; set; }
        public AnimatedSwitcherTransitionMode TransitionMode { get; set; } = AnimatedSwitcherTransitionMode.Parallel;
        public AnimatedSwitcherTransitionBuilder TransitionBuilder { get; set; } = DefaultTransitionBuilder;
        public AnimatedSwitcherLayoutBuilder LayoutBuilder { get; set; } = DefaultLayoutBuilder;

        public override State CreateState() => new AnimatedSwitcherState();

        private static Widget DefaultTransitionBuilder(IAnimation<float> animation, Widget child)
        {
            return new CompositeTransition
            {
                Opacity = animation,
                Child = child,
            };
        }

        private static Widget DefaultLayoutBuilder(Widget currentChild, IEnumerable<Widget> previousChildren)
        {
            var stack = new ZStack
            {
                Alignment = Alignment.Center,
                Children = {previousChildren}
            };

            if (currentChild != null)
            {
                stack.Children.Add(currentChild);
            }

            return stack;
        }
    }

    public class AnimatedSwitcherState : HocState<AnimatedSwitcher>
    {
        private readonly MutableAtom<int> _version = Atom.Value(int.MinValue);
        private readonly List<Entry> _outgoingEntries = new List<Entry>();
        private readonly Queue<AnimationController> _pendingAnimations = new Queue<AnimationController>();

        private List<Widget> _outgoingWidgets = new List<Widget>();
        private Entry _currentEntry;
        private int _childNumber;

        public override void InitState()
        {
            base.InitState();

            AddEntryForNewChild(animate: false);
        }

        public override Widget Build(BuildContext context)
        {
            _version.Get();

            RebuildOutgoingWidgetsIfNeed();

            var previousChildren = _outgoingWidgets.Where(w => w.Key != _currentEntry?.Transition.Key);
            return Widget.LayoutBuilder.Invoke(_currentEntry?.Transition, previousChildren);
        }

        public override void DidUpdateWidget(AnimatedSwitcher oldWidget)
        {
            base.DidUpdateWidget(oldWidget);

            if (Widget.TransitionBuilder != oldWidget.TransitionBuilder)
            {
                foreach (var outgoingEntry in _outgoingEntries)
                {
                    UpdateTransitionForEntry(outgoingEntry);
                }

                if (_currentEntry != null)
                {
                    UpdateTransitionForEntry(_currentEntry);
                }

                MarkChildWidgetCacheAsDirty();
            }

            var hasNewChild = Widget.Child != null;
            var hasOldChild = _currentEntry != null;

            if (hasNewChild != hasOldChild ||
                hasNewChild && !StateUtilities.CanUpdateWidget(Widget.Child, _currentEntry.Child))
            {
                _childNumber += 1;
                AddEntryForNewChild(animate: true);
            }
            else if (_currentEntry != null)
            {
                Assert.IsTrue(hasOldChild && hasNewChild);
                Assert.IsTrue(StateUtilities.CanUpdateWidget(Widget.Child, _currentEntry.Child));

                _currentEntry.Child = Widget.Child;
                UpdateTransitionForEntry(_currentEntry);

                MarkChildWidgetCacheAsDirty();
            }
        }

        private void AddEntryForNewChild(bool animate)
        {
            Assert.IsTrue(animate || _currentEntry == null);

            var hasCurrentEntry = _currentEntry != null;
            if (hasCurrentEntry)
            {
                Assert.IsTrue(animate);
                Assert.IsTrue(!_outgoingEntries.Contains(_currentEntry));

                _outgoingEntries.Add(_currentEntry);

                if (_currentEntry.AnimationController.Status != AnimationStatus.Dismissed)
                {
                    _currentEntry.AnimationController.Reverse();
                }
                else
                {
                    _currentEntry.LifetimeController.Dispose();
                }

                _currentEntry = null;

                MarkChildWidgetCacheAsDirty();
            }

            if (Widget.Child == null)
            {
                return;
            }

            var lc = StateLifetime.CreateNested();
            var controller = new AnimationController(lc.Lifetime, Widget.Duration, Widget.ReverseDuration);

            _currentEntry = NewEntry(lc, Widget.Child, controller, Widget.TransitionBuilder);

            if (animate)
            {
                if (hasCurrentEntry && Widget.TransitionMode == AnimatedSwitcherTransitionMode.Sequential)
                {
                    _pendingAnimations.Enqueue(controller);
                }
                else
                {
                    controller.Forward();
                }
            }
            else
            {
                Assert.IsTrue(_outgoingEntries.Count == 0);
                controller.Complete();
            }
        }

        private Entry NewEntry(ILifetimeController lc, Widget child, AnimationController controller,
            AnimatedSwitcherTransitionBuilder transition)
        {
            var entry = new Entry
            {
                Child = child,
                Transition = MakeTransition(child, controller, transition),
                AnimationController = controller,
                LifetimeController = lc,
            };

            Atom.Reaction(lc.Lifetime, () => controller.Status, status =>
            {
                if (status == AnimationStatus.Dismissed)
                {
                    lc.Dispose();
                }
            }, fireImmediately: false);

            lc.Register(() =>
            {
                _outgoingEntries.Remove(entry);
                _version.Value++;

                MarkChildWidgetCacheAsDirty();

                ForwardPendingAnimation();
            });

            return entry;
        }

        private void ForwardPendingAnimation()
        {
            while (_pendingAnimations.Count > 0)
            {
                var controller = _pendingAnimations.Dequeue();
                if (controller.Lifetime.IsDisposed)
                {
                    continue;
                }

                controller.Forward();
                break;
            }
        }

        private void UpdateTransitionForEntry(Entry entry)
        {
            entry.Transition = MakeTransition(entry.Child, entry.AnimationController, Widget.TransitionBuilder);
        }

        private Builder MakeTransition(Widget child, IAnimation<float> animation,
            AnimatedSwitcherTransitionBuilder transition)
        {
            return new Builder(_ => transition.Invoke(animation, child))
            {
                Key = child.Key ?? Key.Of(_childNumber)
            };
        }

        private void MarkChildWidgetCacheAsDirty()
        {
            _outgoingWidgets = null;
        }

        private void RebuildOutgoingWidgetsIfNeed()
        {
            if (_outgoingWidgets == null)
            {
                _outgoingWidgets = _outgoingEntries
                    .Select(entry => entry.Transition)
                    .ToList();
            }

            Assert.IsTrue(_outgoingEntries.Count == _outgoingWidgets.Count);
            Assert.IsTrue(_outgoingEntries.Count == 0 || _outgoingEntries.Last().Transition == _outgoingWidgets.Last());
        }

        private class Entry
        {
            public ILifetimeController LifetimeController;
            public AnimationController AnimationController;
            public Widget Transition;
            public Widget Child;
        }
    }

    public enum AnimatedSwitcherTransitionMode
    {
        Parallel,
        Sequential,
    }

    public delegate Widget AnimatedSwitcherTransitionBuilder(IAnimation<float> animation, Widget child);

    public delegate Widget AnimatedSwitcherLayoutBuilder(Widget currentChild, IEnumerable<Widget> previousWidget);
}