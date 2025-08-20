using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniMob.UI.Layout;
using UnityEngine.Assertions;

namespace UniMob.UI.Internal
{
    public static class StateUtilities
    {
        public static bool CanUpdateWidget(Widget oldWidget, Widget newWidget)
        {
            return oldWidget.Key == newWidget.Key &&
                   oldWidget.Type == newWidget.Type;
        }

        public static State[] UpdateChildren(BuildContext context, State[] oldChildren, List<Widget> newWidgets)
        {
            var newChildrenTop = 0;
            var oldChildrenTop = 0;
            var newChildrenBottom = newWidgets.Count - 1;
            var oldChildrenBottom = oldChildren.Length - 1;

            var newChildren = oldChildren.Length == newWidgets.Count ? oldChildren : new State[newWidgets.Count];

            // Update the top of the list.
            while ((oldChildrenTop <= oldChildrenBottom) && (newChildrenTop <= newChildrenBottom))
            {
                var oldChild = oldChildren[oldChildrenTop];
                var newWidget = newWidgets[newChildrenTop];

                if (!CanUpdateWidget(oldChild.RawWidget, newWidget))
                    break;

                var newChild = UpdateChild(context, oldChild, newWidget);
                newChildren[newChildrenTop] = newChild;
                newChildrenTop += 1;
                oldChildrenTop += 1;
            }


            // Scan the bottom of the list.
            while ((oldChildrenTop <= oldChildrenBottom) && (newChildrenTop <= newChildrenBottom))
            {
                var oldChild = oldChildren[oldChildrenBottom];
                var newWidget = newWidgets[newChildrenBottom];

                if (!CanUpdateWidget(oldChild.RawWidget, newWidget))
                    break;

                oldChildrenBottom -= 1;
                newChildrenBottom -= 1;
            }

            // Scan the old children in the middle of the list.
            var haveOldChildren = oldChildrenTop <= oldChildrenBottom;
            Dictionary<Key, State> oldKeyedChildren = null;
            if (haveOldChildren)
            {
                oldKeyedChildren = Pools.KeyToState.Get();
                while (oldChildrenTop <= oldChildrenBottom)
                {
                    var oldChild = oldChildren[oldChildrenTop];
                    if (oldChild.RawWidget.Key != null)
                    {
                        oldKeyedChildren[oldChild.RawWidget.Key] = oldChild;
                    }
                    else
                    {
                        DeactivateChild(oldChild);
                    }

                    oldChildrenTop += 1;
                }
            }

            // Update the middle of the list.
            while (newChildrenTop <= newChildrenBottom)
            {
                State oldChild = null;
                var newWidget = newWidgets[newChildrenTop];
                if (haveOldChildren)
                {
                    var key = newWidget.Key;
                    if (key != null)
                    {
                        if (oldKeyedChildren.TryGetValue(key, out oldChild))
                        {
                            if (CanUpdateWidget(oldChild.RawWidget, newWidget))
                            {
                                // we found a match!
                                // remove it from oldKeyedChildren so we don't unsync it later
                                oldKeyedChildren.Remove(key);
                            }
                            else
                            {
                                // Not a match, let's pretend we didn't see it for now.
                                oldChild = null;
                            }
                        }
                    }
                }

                var newChild = UpdateChild(context, oldChild, newWidget);
                newChildren[newChildrenTop] = newChild;
                newChildrenTop += 1;
            }

            // We've scanned the whole list.
            Assert.IsTrue(oldChildrenTop == oldChildrenBottom + 1);
            Assert.IsTrue(newChildrenTop == newChildrenBottom + 1);
            Assert.IsTrue(newWidgets.Count - newChildrenTop == oldChildren.Length - oldChildrenTop);
            newChildrenBottom = newWidgets.Count - 1;
            oldChildrenBottom = oldChildren.Length - 1;

            // Update the bottom of the list.
            while ((oldChildrenTop <= oldChildrenBottom) && (newChildrenTop <= newChildrenBottom))
            {
                var oldChild = oldChildren[oldChildrenTop];
                var newWidget = newWidgets[newChildrenTop];
                var newChild = UpdateChild(context, oldChild, newWidget);
                newChildren[newChildrenTop] = newChild;
                newChildrenTop += 1;
                oldChildrenTop += 1;
            }

            // Clean up any of the remaining middle nodes from the old list.
            if (haveOldChildren && oldKeyedChildren.Count > 0)
            {
                foreach (var pair in oldKeyedChildren)
                {
                    var oldChild = pair.Value;
                    DeactivateChild(oldChild);
                }
            }

            if (oldKeyedChildren != null)
            {
                Pools.KeyToState.Recycle(oldKeyedChildren);
            }

            return newChildren;
        }

        public static void DeactivateChild([NotNull] State child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            Assert.IsNull(Atom.CurrentScope);

            if (child.RawWidget.Key is GlobalKey globalKey)
            {
                globalKey.UntypedCurrentState = null;
            }

            child.Dispose();
        }

        public static State UpdateChild(BuildContext context, [CanBeNull] State child, [NotNull] Widget newWidget)
        {
            Assert.IsNull(Atom.CurrentScope);
            
            
            

            if (child != null)
            {
                if (child.RawWidget == newWidget)
                {
                    return child;
                }

                if (CanUpdateWidget(child.RawWidget, newWidget))
                {
                    child.Update(newWidget);
                    return child;
                }

                DeactivateChild(child);
            }
            
            return InflateWidget(context, newWidget);
        }

        public static State InflateWidget(BuildContext context, [NotNull] Widget newWidget)
        {
            if (newWidget == null) throw new ArgumentNullException(nameof(newWidget));
            Assert.IsNull(Atom.CurrentScope);

            var newChild = newWidget.CreateState();
            if (newChild == null)
            {

                var providerSource = context.FindAncestorStateImplementing<IStateProviderSource>();
                if (providerSource != null)
                {
                    newChild = providerSource.StateProvider.Of(newWidget);
                }
                else
                {
                    throw new InvalidOperationException($"Widget {newWidget} requires a StateProvider, but no IStateProviderSource was found in the context.");
                }
            }


            newChild.Mount(context);
            newChild.Update(newWidget);

            if (newWidget.Key is GlobalKey globalKey)
            {
                globalKey.UntypedCurrentState = newChild;
            }

            newChild.InitState();
            return newChild;
        }
    }
}