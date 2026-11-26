using UnityEngine;

public class IceOrb : OrbitOrb
{
    public float freezeDuration = 2f;

    public override void OnHit(Collider2D other, IHittable hittable)
    {
        base.OnHit(other, hittable);
        var handler = (hittable as UnityEngine.Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new IceEffect(freezeDuration));
    }
}
