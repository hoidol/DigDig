using UnityEngine;

public class FreezeOnHitBehavior : IBulletBehavior
{
    float chance;   // 0~1
    float duration;

    public FreezeOnHitBehavior(float chance, float duration)
    {
        this.chance = chance;
        this.duration = duration;
    }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (Random.value <= chance)
        {
            StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
            handler?.Apply(new FreezeEffect(duration));
        }
        return true;
    }

    public void OnMove(BulletBase bullet) { }

    public void Merge(IBulletBehavior other)
    {
        FreezeOnHitBehavior o = (FreezeOnHitBehavior)other;
        if (o.chance > chance) chance = o.chance;
        if (o.duration > duration) duration = o.duration;
    }
}
