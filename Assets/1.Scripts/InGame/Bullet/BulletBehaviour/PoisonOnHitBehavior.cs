using UnityEngine;

public class PoisonOnHitBehavior : IBulletBehavior
{
    float duration;
    float damagePerTick;

    public PoisonOnHitBehavior(float duration, float damagePerTick)
    {
        this.duration = duration;
        this.damagePerTick = damagePerTick;
    }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new PoisonEffect(duration, damagePerTick));
        return true;
    }

    public void OnMove(BulletBase bullet) { }

    public void Merge(IBulletBehavior other)
    {
        PoisonOnHitBehavior o = (PoisonOnHitBehavior)other;
        if (o.damagePerTick > damagePerTick) damagePerTick = o.damagePerTick;
        if (o.duration > duration) duration = o.duration;
    }
}
