using UnityEngine;

// 보스/엘리트 제외하고 적중 시 스턴
public class StunOnHitBehavior : IBulletBehavior
{
    float duration;
    public StunOnHitBehavior(float d)
    {
        duration = d;
    }
    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (hit is Enemy enemy && hit is not Boss)
            enemy.GetComponent<StatusEffectHandler>()?.Apply(new StunEffect(duration));
        return true;
    }

    public void OnMove(BulletBase bullet) { }
    public void Merge(IBulletBehavior other) { }
}
