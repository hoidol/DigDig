using UnityEngine;

public class BurnOnHitBehavior : IBulletBehavior
{
    float duration;
    float dps;

    public BurnOnHitBehavior(float duration, float dps)
    {
        this.duration = duration;
        this.dps = dps;
    }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new BurnEffect(duration, dps));
        return true;
    }

    public void OnMove(BulletBase bullet) { }

    public void Merge(IBulletBehavior other)
    {
        BurnOnHitBehavior o = (BurnOnHitBehavior)other;
        if (o.dps > dps) dps = o.dps;
        if (o.duration > duration) duration = o.duration;
    }
}
