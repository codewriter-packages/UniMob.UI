using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_VerticalSplitBox",
    typeof(RectTransform), typeof(VerticalSplitBoxView))]

namespace UniMob.UI.Widgets
{
    public class VerticalSplitBoxView : View<IVerticalSplitBoxState>
    {
        private ViewMapperBase _mapper;

        protected override void Activate()
        {
            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);

            base.Activate();
        }

        protected override void Render()
        {
            using (var render = _mapper.CreateRender())
            {
                var firstChild = State.FirstChild;
                var secondChild = State.SecondChild;

                var firstSize = firstChild.Size.GetSizeUnbounded();
                var secondSize = secondChild.Size.GetSizeUnbounded();

                var firstStretched = float.IsInfinity(firstSize.y);
                var secondStretched = float.IsInfinity(secondSize.y);

                var firstView = render.RenderItem(firstChild);
                var secondView = render.RenderItem(secondChild);

                if (firstStretched)
                {
                    if (secondStretched)
                    {
                        DoChildLayout(firstView, firstSize, Alignment.TopCenter, Vector2.zero);
                        DoChildLayout(secondView, secondSize, Alignment.TopCenter, Vector2.zero);

                        var rt1 = firstView.rectTransform;
                        var rt2 = secondView.rectTransform;
                        rt1.anchorMin = new Vector2(rt1.anchorMin.x, 0.5f);
                        rt2.anchorMax = new Vector2(rt2.anchorMax.x, 0.5f);
                    }
                    else
                    {
                        DoChildLayout(firstView, firstSize, Alignment.BottomCenter, Vector2.zero);
                        DoChildLayout(secondView, secondSize, Alignment.BottomCenter, Vector2.zero);

                        ViewLayoutUtility.SetPadding(firstView.rectTransform,
                            new RectPadding(0, 0, 0, secondSize.y));
                    }
                }
                else
                {
                    if (secondStretched)
                    {
                        DoChildLayout(firstView, firstSize, Alignment.TopCenter, Vector2.zero);
                        DoChildLayout(secondView, secondSize, Alignment.TopCenter, Vector2.zero);

                        ViewLayoutUtility.SetPadding(secondView.rectTransform,
                            new RectPadding(0, 0, firstSize.y, 0));
                    }
                    else
                    {
                        DoChildLayout(firstView, firstSize, Alignment.TopCenter, Vector2.zero);
                        DoChildLayout(secondView, secondSize, Alignment.TopCenter, new Vector2(0, firstSize.y));
                    }
                }
            }
        }

        private static void DoChildLayout(IView view, Vector2 size, Alignment alignment, Vector2 position)
        {
            LayoutData layout;
            layout.Size = size;
            layout.Alignment = alignment;
            layout.Corner = alignment;
            layout.CornerPosition = position;
            ViewLayoutUtility.SetLayout(view.rectTransform, layout);
        }
    }

    public interface IVerticalSplitBoxState : IViewState
    {
        IState FirstChild { get; }
        IState SecondChild { get; }
    }
}