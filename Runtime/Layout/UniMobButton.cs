using System;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    /// <summary>
    /// A widget that detects click interactions.
    /// </summary>
    public class UniMobButton : LayoutWidget
    {
        public bool Interactable { get; set; } = true;
        public Action? OnClick { get; set; }
        public Widget Child { get; set; }

        public override State CreateState() => new UniMobButtonState();

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderProxy((UniMobButtonState) state);
        }
    }

    // The State now implements the contract for RenderAlign (IAlignState)
    // and the contract for its View (IUniMobButtonState).
    internal class UniMobButtonState : LayoutState<UniMobButton>, 
        Widgets.IUniMobButtonState, ISingleChildLayoutState
    {
        private readonly StateHolder _child;
        public IState Child => _child.Value;
        
        public Alignment Alignment => Alignment.Center;

        public UniMobButtonState()
        {
            _child = CreateChild(context => Widget.Child);
        }
        
        // --- IUniMobButtonState Implementation (for the View) ---
        public override WidgetViewReference View => WidgetViewReference.Resource("UniMob.Button");
        public bool Interactable => Widget.Interactable;
        public void OnClick()
        {
            using (Atom.NoWatch)
            {
                Widget.OnClick?.Invoke();
            }
        }
    }

}