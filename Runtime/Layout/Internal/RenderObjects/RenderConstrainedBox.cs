using System;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal interface IConstrainedBoxState : ISingleChildLayoutState
    {
        LayoutConstraints BoxConstraints { get; }
    }

    internal class RenderConstrainedBox : RenderProxy
    {
        private readonly IConstrainedBoxState _state;

        public RenderConstrainedBox(IConstrainedBoxState state) : base(state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var selfConstraints = _state.BoxConstraints;
            var childConstraints = constraints.Enforce(selfConstraints);

            if (_state.Child == null)
                return childConstraints.Constrain(Vector2.zero);

            ChildSize = LayoutChild(_state.Child, childConstraints);
            return ChildSize;
        }


        // Intrinsic sizing is also delegated directly to the child, but constrained
        // by the BoxConstraints.
        public override float GetIntrinsicWidth(float height)
        {
            var selfConstraints = _state.BoxConstraints;
            if (selfConstraints.HasTightWidth)
                return selfConstraints.MinWidth;
            
            var childIntrinsicWidth = base.GetIntrinsicWidth(height);
            return selfConstraints.ConstrainWidth(childIntrinsicWidth);
        }

        public override float GetIntrinsicHeight(float width)
        {
            var selfConstraints = _state.BoxConstraints;
            if (selfConstraints.HasTightWidth)
                return selfConstraints.MinWidth;

            var childIntrinsicHeight = base.GetIntrinsicHeight(width);
            return selfConstraints.ConstrainHeight(childIntrinsicHeight);
        }

        
    }
}