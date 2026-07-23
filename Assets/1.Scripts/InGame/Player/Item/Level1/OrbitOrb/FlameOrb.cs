public class FlameOrb : OrbitOrb
{
    public float burnDuration = 3f;
    public float burnDps      = 5f;

    protected override void OnHit(IHittable hittable)
    {
        base.OnHit(hittable);
        var handler = (hittable as UnityEngine.Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new BurnEffect(burnDuration, burnDps));
    }
}
