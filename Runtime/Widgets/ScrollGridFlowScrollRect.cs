using UnityEngine;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    [RequireComponent(typeof(ScrollGridFlowView))]
    public sealed class ScrollGridFlowScrollRect : ScrollRect
    {
        [SerializeField] private ScrollGridFlowView scrollGridFlowView = default;
        
        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);
            
            scrollGridFlowView.OnContentAnchoredPositionChanged();
        }
    }
}