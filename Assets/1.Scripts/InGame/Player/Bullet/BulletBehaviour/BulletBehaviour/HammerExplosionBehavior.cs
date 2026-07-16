using UnityEngine;

//
public class HammerExplosionBehavior : IBulletBehavior
{
    float chance;
    float radius;
    float damage;
    LayerMask enemyLayer;

    public HammerExplosionBehavior(float chance, float radius, float damage, LayerMask enemyLayer)
    {
        this.chance = chance;
        this.radius = radius;
        this.damage = damage;
        this.enemyLayer = enemyLayer;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (hit is Stone && Random.value <= chance)
        {
            InGameUtil.DamageEnemies(bullet.transform.position, radius, damage, enemyLayer);
        }
        return true;
    }

}
