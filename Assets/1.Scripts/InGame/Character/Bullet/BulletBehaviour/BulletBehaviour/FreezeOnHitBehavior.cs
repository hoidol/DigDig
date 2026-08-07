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

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (Random.value <= chance)
        {
            StatusEffectHandler handler = (hit as Component)?.GetComponent<StatusEffectHandler>();
            handler?.Apply(new FreezeEffect(duration));
        }
        return true;
    }

}
