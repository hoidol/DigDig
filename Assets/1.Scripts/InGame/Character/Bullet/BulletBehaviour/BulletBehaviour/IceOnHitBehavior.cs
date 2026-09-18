using UnityEngine;

public class IceOnHitBehavior : IBulletBehavior
{
    float duration;
    float chance;

    public IceOnHitBehavior(float duration,float chance)
    {
        this.duration = duration;
        this.chance= chance;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if(Random.value >= chance)
            return true;
        StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new IceEffect(duration));
        return true;
    }

}
