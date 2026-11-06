using UnityEngine;

// 낙인 슬라임: 적중 시 chance 확률로 낙인 1중첩 (StigmaEffect)
public class StigmaOnHitBehavior : IBulletBehavior
{
    public int ApplyOrder => 0;
    readonly float chance;
    readonly int maxStack;
    readonly float damagePerStack;

    public StigmaOnHitBehavior(float chance, int maxStack, float damagePerStack)
    {
        this.chance = chance;
        this.maxStack = maxStack;
        this.damagePerStack = damagePerStack;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (Random.value <= chance && hit is Enemy enemy && enemy.CurHp > 0)
            enemy.ApplyStatusEffect(new StigmaEffect(maxStack, damagePerStack));
        return true;
    }
}
