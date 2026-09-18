using UnityEngine;

public class BoomBehaviour : IBulletBehavior
{
    readonly float radius;
    readonly float damage;
    readonly LayerMask layer;
    readonly int maxHitCount;

    public BoomBehaviour(float radius, float damage,int maxHitCount, LayerMask layer)
    {
        this.radius = radius;
        this.damage = damage;
        this.maxHitCount= maxHitCount;
        this.layer = layer;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        InGameUtil.DamageEnemies(bullet.transform.position, radius, damage, layer,maxHitCount);
        return false;
    }

}
