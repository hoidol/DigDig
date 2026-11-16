using UnityEngine;

// 낙인 슬라임: 적중 시 chance 확률로 낙인 (StigmaEffect, 중첩 없음)
public class StigmaOnHitBehavior : IBulletBehavior
{
    public int ApplyOrder => 0;
    readonly float chance;
    readonly float damageBonus;

    public StigmaOnHitBehavior(float chance, float damageBonus)
    {
        this.chance = chance;
        this.damageBonus = damageBonus;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (Random.value <= chance && hit is Enemy enemy && enemy.CurHp > 0)
            enemy.ApplyStatusEffect(new StigmaEffect(damageBonus));
        return true;
    }
}
