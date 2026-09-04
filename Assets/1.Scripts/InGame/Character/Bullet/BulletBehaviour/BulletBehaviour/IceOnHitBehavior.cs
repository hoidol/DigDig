using UnityEngine;

public class IceOnHitBehavior : IBulletBehavior
{
    float duration;

    public IceOnHitBehavior(float duration)
    {
        this.duration = duration;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new IceEffect(duration));
        return true;
    }

}
