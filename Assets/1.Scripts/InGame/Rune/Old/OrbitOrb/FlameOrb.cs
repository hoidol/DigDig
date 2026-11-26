using UnityEngine;

public class FlameOrb : OrbitOrb
{
    public float burnDuration = 3f;
    public float burnDps      = 5f;

    public override void OnHit(Collider2D other, IHittable hittable)
    {
        base.OnHit(other, hittable);
        var handler = (hittable as UnityEngine.Component)?.GetComponent<StatusEffectHandler>();
        handler?.Apply(new FlameEffect(burnDuration, burnDps));
    }
}
