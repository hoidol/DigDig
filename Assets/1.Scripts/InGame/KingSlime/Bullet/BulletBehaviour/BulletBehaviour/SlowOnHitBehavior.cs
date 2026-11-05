using UnityEngine;

public class SlowOnHitBehavior : IBulletBehavior
{
public int ApplyOrder => 0;    readonly float chance;
    readonly float duration;

    public SlowOnHitBehavior(float chance, float duration)
    {
        this.chance = chance;
        this.duration = duration;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (Random.value <= chance && hit is Enemy enemy)
        {
            var handler = enemy.GetComponent<StatusEffectHandler>();
            if (handler != null) handler.Apply(new SlowEffect(duration));
        }
        return true;
    }


}
