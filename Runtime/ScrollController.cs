namespace UniMob.UI
{
    public class ScrollController : ILifetimeScope
    {
        public ScrollController(Lifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public Lifetime Lifetime { get; }

        [Atom] public float NormalizedValue { get; internal set; }
    }
}