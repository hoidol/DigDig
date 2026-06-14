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

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new BurnEffect(duration, dps));
        return true;
    }

}
