using UnityEngine;

public class BoomBehaviour : IBulletBehavior
{
    readonly float radius;
    readonly float damage;
    readonly LayerMask layer;

    public BoomBehaviour(float radius, float damage, LayerMask layer)
    {
        this.radius = radius;
        this.damage = damage;
        this.layer = layer;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        InGameUtil.DamageEnemies(bullet.transform.position, radius, damage, layer);
        return true;
    }

}
