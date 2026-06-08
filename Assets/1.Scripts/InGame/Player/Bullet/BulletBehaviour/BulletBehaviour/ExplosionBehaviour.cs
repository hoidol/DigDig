using UnityEngine;

public class ExplosionBehaviour : IBulletBehavior
{
    readonly float radius;
    readonly float damage;
    readonly LayerMask layer;

    public ExplosionBehaviour(float radius, float damage, LayerMask layer)
    {
        this.radius = radius;
        this.damage = damage;
        this.layer = layer;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D)
    {
        AOEUtil.DamageEnemies(bullet.transform.position, radius, damage, layer);
        return true;
    }

    public void OnMove(BulletObject bullet) { }
    public void Merge(IBulletBehavior other) { }
}
