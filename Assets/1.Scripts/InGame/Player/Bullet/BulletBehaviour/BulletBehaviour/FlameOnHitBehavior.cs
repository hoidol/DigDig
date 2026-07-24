using UnityEngine;

public class FlameOnHitBehavior : IBulletBehavior
{
    float duration;
    float dps;

    public FlameOnHitBehavior(float duration, float dps)
    {
        this.duration = duration;
        this.dps = dps;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new FlameEffect(duration, dps));
        return true;
    }

}
