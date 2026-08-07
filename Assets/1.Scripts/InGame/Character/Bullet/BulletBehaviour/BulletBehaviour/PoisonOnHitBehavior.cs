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

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new PoisonEffect(duration, damagePerTick));
        return true;
    }

}
