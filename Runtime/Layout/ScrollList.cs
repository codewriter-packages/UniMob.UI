using System.Collections.Generic;
using JetBrains.Annotations;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Internal.Views;
using UnityEngine.UI;
using ScrollListView = UniMob.UI.Layout.Internal.Views.ScrollListView;
using Vector2 = UnityEngine.Vector2;

namespace UniMob.UI.Layout
{
    public class ScrollList : LayoutWidget
    {
        public List<Widget> Children { get; set; } = new();
        public Axis Axis { get; set; } = Axis.Vertical;
        public ScrollController ScrollController { get; set; }

        public bool UseMask { get; set; } = true;
        public ScrollRect.MovementType? MovementType { get; set; }


        public override State CreateState()
        {
            return new ScrollListState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderSliverList((ISliverState) state);
        }
    }


    // The state implements both the generic interface for the View (IMultiChildLayoutState)
    // and our specific interface for the RenderObject (IScrollingListState).
    // The reason is that the RenderObject needs the entire children collection (visible AND invisible) to correctly
    // compute the layout, while the View only needs the visible children to render the UI.
    public class ScrollListState : LayoutState<ScrollList>, ISliverState, IScrollingListState
    {
        private readonly StateCollectionHolder _allChildren;
        private readonly Dictionary<Key, int> _childKeyToIndexMap = new();
        private readonly Atom<IState[]> _visibleChildren;

        // A reactive atom holding the INDICES of the visible children.
        private readonly MutableAtom<List<int>> _visibleIndices = Atom.Value(new List<int>());


        [CanBeNull] private ScrollListView _view;

        public ScrollListState()
        {
            _allChildren = CreateChildren(context =>
            {
                var children = Widget.Children;
                _childKeyToIndexMap.Clear();
                for (var i = 0; i < children.Count; i++)
                {
                    var key = children[i].Key;
                    if (key != null) _childKeyToIndexMap.Add(key, i);
                }

                return children;
            });

            _visibleChildren = Atom.Computed(StateLifetime, () =>
            {
                var indices = _visibleIndices.Value;
                var all = _allChildren.Value;
                var visible = new IState[indices.Count];

                for (var i = 0; i < indices.Count; i++)
                {
                    var index = indices[i];
                    if (index < all.Length) visible[i] = all[index];
                }

                return visible;
            });
        }

        [Atom]
        public IState[] Children => _visibleChildren.Value;

        public bool UseMask => Widget.UseMask;
        public ScrollRect.MovementType MovementType => Widget.MovementType ?? ScrollRect.MovementType.Elastic;


        [Atom] public ScrollController ScrollController { get; private set; }


        // These properties are required by the ISliverListState interface.
        // The RenderSliverList will read them during layout.
        [Atom] public Vector2 ViewportSize { get; set; }
        [Atom] public float ScrollOffset { get; set; }


        [Atom]
        public IState[] AllChildren => _allChildren.Value;

        public Axis Axis => Widget.Axis;

        void ISliverState.SetVisibleChildren(List<IndexedLayoutData> visibleChildren)
        {
            var indices = new List<int>(visibleChildren.Count);
            for (var i = 0; i < visibleChildren.Count; i++) indices.Add(visibleChildren[i].ChildIndex);

            using (Atom.NoWatch)
            {
                _visibleIndices.Value = new List<int>(indices);
            }
        }

        public override void DidViewMount(IView view)
        {
            base.DidViewMount(view);
            _view = view as ScrollListView;
        }

        public override void DidViewUnmount(IView view)
        {
            base.DidViewUnmount(view);
            _view = null;
        }

        public override WidgetViewReference View => WidgetViewReference.Resource("Layout/UniMob.ScrollList");

        public override void InitState()
        {
            base.InitState();

            // Use the provided controller or create a new one.
            ScrollController = Widget.ScrollController ?? new ScrollController(StateLifetime);
        }

        public override void DidUpdateWidget(ScrollList oldWidget)
        {
            base.DidUpdateWidget(oldWidget);

            if (Widget.ScrollController != null && Widget.ScrollController != ScrollController)
                ScrollController = Widget.ScrollController;
        }


        public bool ScrollTo(int index)
        {
            return ScrollTo(index, 0);
        }

        public bool ScrollTo(Key key)
        {
            return ScrollTo(key, 0);
        }

        public bool ScrollTo(int index, float duration, ScrollToPosition? position = null, Easing easing = null)
        {
            return _view?.ScrollTo(index, duration, position ?? ScrollToPosition.Start, easing ?? Ease.InOutCirc) ??
                   false;
        }


        public bool ScrollTo(Key key, float duration, ScrollToPosition? position = null, Easing easing = null)
        {
            if (!_childKeyToIndexMap.TryGetValue(key, out var index))
                return false;

            return ScrollTo(index, duration, position, easing);
        }
    }
}