using UnityEngine;

// 적중 시 chance(0~1) 확률로 스턴 (chance 생략 시 항상). 엘리트/보스는 시간 절반 (StatusEffectHandler)
public class StunOnHitBehavior : IBulletBehavior
{
    public int ApplyOrder => 0;
    readonly float duration;
    readonly float chance;

    public StunOnHitBehavior(float d, float chance = 1f)
    {
        duration = d;
        this.chance = chance;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (chance < 1f && Random.value >= chance)
            return true;
        if (hit is Enemy enemy) // 엘리트/보스는 StatusEffectHandler에서 시간 절반
            enemy.GetComponent<StatusEffectHandler>()?.Apply(new StunEffect(duration));
        return true;
    }

}
