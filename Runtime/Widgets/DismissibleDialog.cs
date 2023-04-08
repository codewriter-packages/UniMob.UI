using System;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public class DismissibleDialog : SingleChildLayoutWidget
    {
        public override State CreateState() => new DismissibleDialogState();

        public float DismissTreshold { get; set; } = 0.1f;
        public float? CollapsedHeight { get; set; }

        public Action OnExpand { get; set; }
        public Action OnCollapse { get; set; }
        public Action OnDismiss { get; set; }
    }

    [AtomContainer]
    internal class DismissibleDialogState : SingleChildLayoutState<DismissibleDialog>, IDismissibleDialogState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.DismissibleDialog");

        [Atom] private bool ExpandedByUser { get; set; }
        [Atom] private float? CollapsedHeight => Widget.CollapsedHeight;
        [Atom] private float? ExpandedHeight { get; set; }

        [Atom] public float Offset { get; private set; }
        [Atom] public bool Expanded => ExpandedByUser || Widget.CollapsedHeight == null;
        [Atom] public bool Dismissed { get; private set; }
        [Atom] public float DismissThreshold => Widget.DismissTreshold;

        [Atom] public WidgetSize ChildSize
        {
            get
            {
                if (Expanded || CollapsedHeight == null)
                {
                    return ExpandedHeight.HasValue
                        ? WidgetSize.FixedHeight(ExpandedHeight.Value - Offset)
                        : WidgetSize.Stretched;
                }

                return WidgetSize.FixedHeight(CollapsedHeight.Value - Offset);
            }
        }

        public void SetOffset(float offset)
        {
            if (Dismissed)
            {
                return;
            }

            Offset = offset;

            if (ExpandedHeight.HasValue && CollapsedHeight.HasValue &&
                !Expanded && -offset >= (ExpandedHeight - CollapsedHeight) * 0.99f)
            {
                OnExpand();
            }
            else if (ExpandedHeight.HasValue && CollapsedHeight.HasValue &&
                     Expanded && offset >= (ExpandedHeight - CollapsedHeight) * 0.99f)
            {
                OnCollapse();
            }
            else if (Expanded)
            {
                Offset = Mathf.Max(0, Offset);
            }
        }

        public void SetExpandedHeight(float height)
        {
            ExpandedHeight = height;
        }

        public void OnExpand()
        {
            if (CollapsedHeight == null || ExpandedHeight == null)
            {
                return;
            }

            Offset += ExpandedHeight.Value - CollapsedHeight.Value;
            ExpandedByUser = true;

            Widget.OnExpand?.Invoke();
        }

        public void OnCollapse()
        {
            if (CollapsedHeight == null || ExpandedHeight == null)
            {
                OnDismiss();
                return;
            }

            Offset -= ExpandedHeight.Value - CollapsedHeight.Value;
            ExpandedByUser = false;

            Widget.OnCollapse?.Invoke();
        }

        public void OnDismiss()
        {
            Dismissed = true;

            Widget.OnDismiss?.Invoke();
        }
    }
}