using UnityEngine;

public class IceOnHitBehavior : IBulletBehavior
{public int ApplyOrder => 0;
    float duration;
    float chance;
    float slowRate;

    public IceOnHitBehavior(float duration, float chance, float slowRate = IceEffect.DEFAULT_SLOW_RATE)
    {
        this.duration = duration;
        this.chance = chance;
        this.slowRate = slowRate;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if(Random.value >= chance)
            return true;
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new IceEffect(duration, slowRate));
        return true;
    }

}
