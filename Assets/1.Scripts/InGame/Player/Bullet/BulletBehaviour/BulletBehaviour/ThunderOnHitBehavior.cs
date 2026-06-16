using UnityEngine;

// 탄 적중 시 플레이어 주변 가장 가까운 적/광석에 낙뢰 (ThunderItem 방식)
public class ThunderOnHitBehavior : IBulletBehavior
{
    readonly float searchRadius;
    readonly float strikeRadius;
    readonly float damageRate;

    public ThunderOnHitBehavior(float searchRadius = 8f, float strikeRadius = 1.5f, float damageRate = 1f)
    {
        this.searchRadius = searchRadius;
        this.strikeRadius = strikeRadius;
        this.damageRate = damageRate;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        Vector2 target = (Vector2)bullet.transform.position + Random.insideUnitCircle * searchRadius;
        //Vector2 target = FindTarget(bullet.hitLayerMask);
        float damage = Player.Instance.statMgr.AttackPower * damageRate;
        AOEUtil.DamageEnemies(target, strikeRadius, damage, bullet.hitLayerMask);
        EffectManager.Instance.Play(EffectType.Spark, target);
        return true;
    }

    // Vector2 FindTarget(LayerMask layer)
    // {
    //     Vector2 playerPos = Player.Instance.transform.position;
    //     Collider2D[] cols = Physics2D.OverlapCircleAll(playerPos, searchRadius, layer);

    //     Enemy nearestEnemy = null;
    //     OreStone nearestOre = null;
    //     float enemyDistSq = float.MaxValue;
    //     float oreDistSq = float.MaxValue;

    //     foreach (var col in cols)
    //     {
    //         float distSq = ((Vector2)col.transform.position - playerPos).sqrMagnitude;
    //         if (col.TryGetComponent(out Enemy enemy) && enemy.CurHp > 0)
    //         {
    //             if (distSq < enemyDistSq) { enemyDistSq = distSq; nearestEnemy = enemy; }
    //         }
    //         else if (col.TryGetComponent(out OreStone ore) && ore.curHp > 0)
    //         {
    //             if (distSq < oreDistSq) { oreDistSq = distSq; nearestOre = ore; }
    //         }
    //     }

    //     if (nearestEnemy != null) return nearestEnemy.transform.position;
    //     if (nearestOre != null) return nearestOre.transform.position;
    //     return playerPos + Random.insideUnitCircle.normalized * (searchRadius * Random.Range(0.3f, 1f));
    // }

}
