using UnityEngine;

// 처치한 적 위치에서 범위 폭발
public class KillExplosionBehavior : IBulletBehavior
{
    float radius;
    float damage;
    LayerMask enemyLayer;

    public KillExplosionBehavior(float radius, float damage, LayerMask enemyLayer)
    {
        this.radius = radius;
        this.damage = damage;
        this.enemyLayer = enemyLayer;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (hit is Enemy enemy && enemy.curHp <= 0)
        {
            Debug.Log($"KillExplosionBehavior OnHit {damage}");
            InGameUtil.DamageEnemies(enemy.transform.position, radius, damage, enemyLayer);
        }

        return true;
    }

}
