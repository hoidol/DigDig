using UnityEngine;

public class HammerExplosionBehavior : IBulletBehavior
{
    float chance;
    float radius;
    float damage;
    LayerMask enemyLayer;

    public HammerExplosionBehavior(float chance, float radius, float damage, LayerMask enemyLayer)
    {
        this.chance     = chance;
        this.radius     = radius;
        this.damage     = damage;
        this.enemyLayer = enemyLayer;
    }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (hit is OreStone && Random.value <= chance)
        {
            AOEUtil.DamageEnemies(bullet.transform.position, radius, damage, enemyLayer);
        }
        return true;
    }

    public void OnMove(BulletBase bullet) { }

    public void Merge(IBulletBehavior other)
    {
        HammerExplosionBehavior o = (HammerExplosionBehavior)other;
        if (o.chance > chance) chance = o.chance;
        if (o.damage > damage) damage = o.damage;
    }
}
