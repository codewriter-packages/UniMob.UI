using UniMob.UI.Internal;
using UniMob.UI.Layout;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    [AddComponentMenu("UniMob/Views/ViewPanel")]
    public sealed class ViewPanel : View<IViewState>
    {
        private Atom<LayoutConstraints> _layoutConstraints;

        private ViewMapperBase _mapper;

        internal override bool TriggerViewMountEvents => false;

        public void Render(IState state, bool link = false)
        {
            _layoutConstraints ??= CreateLayoutConstraints();

            // Update the layout constraints. It is required to be here because
            // in the base View.DoRender() we do PerformLayout() and constraints musts be valid.
            state.UpdateConstraints(_layoutConstraints.Get());

            base.Render(state.InnerViewState, link);
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            using (var render = _mapper.CreateRender())
            {
                var child = State;

                Vector2 finalSize;
                if (child is ILayoutState layoutState)
                {
                    // If the root widget is layout-aware, we MUST use its RenderObject's size.
                    // The PerformLayout() call was already handled in the base View.DoRender().
                    finalSize = layoutState.RenderObject.Size;
                }
                else
                {
                    // Fallback for a legacy root widget.
                    finalSize = child.Size.GetSizeUnbounded();
                }

                var childView = render.RenderItem(child);


                LayoutData layout;
                layout.Size = finalSize;
                layout.Alignment = Alignment.Center;
                layout.Corner = Alignment.Center;
                layout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
            }
        }

        private Atom<LayoutConstraints> CreateLayoutConstraints()
        {
            // In the Unity UI we cannot calculate the size immediately (it will be broken sometimes),
            // therefore we use an atom for delayed computation and invalidate it when necessary.
            return Atom.Computed(ViewLifetime, () =>
            {
                var panelSize = rectTransform.rect.size;
                var panelConstraints = LayoutConstraints.Tight(panelSize.x, panelSize.y);
                return panelConstraints;
            });
        }

        private void InvalidateLayoutConstraints()
        {
            using (Atom.NoWatch)
            {
                _layoutConstraints?.Invalidate();
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            InvalidateLayoutConstraints();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            InvalidateLayoutConstraints();
        }
    }
}