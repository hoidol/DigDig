public class IceOrb : OrbitOrb
{
    public float freezeDuration = 2f;

    protected override void OnHit(IHittable hittable)
    {
        base.OnHit(hittable);
        var handler = (hittable as UnityEngine.Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new FreezeEffect(freezeDuration));
    }
}
