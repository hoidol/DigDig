using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// 탄 적중 시 플레이어 주변 가장 가까운 적/광석에 낙뢰 (ThunderItem 방식)
public class ThunderOnHitBehavior : IBulletBehavior
{
    readonly float searchRadius;
    readonly int strikeCount;
    readonly float damageRate;

    public ThunderOnHitBehavior(float searchRadius = 8f, int sCount = 1, float damageRate = 1f)
    {
        this.searchRadius = searchRadius;
        this.strikeCount = sCount;
        this.damageRate = damageRate;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        //Vector2 target = (Vector2)bullet.transform.position + Random.insideUnitCircle * searchRadius;
        Collider2D[] targets = FindTarget(hit2D.point, bullet.hitLayerMask);
        float damage = Player.Instance.statMgr.AttackPower * damageRate;
        DamageData damageData = new DamageData();
        damageData.damage = damage;
        for(int i = 0; i < targets.Length; i++)
        {
            EffectManager.Instance.Play(EffectType.Spark, targets[i].transform.position);    
            targets[i].GetComponent<IHittable>().TakeDamage(damageData);
        }

        
        // AOEUtil.DamageEnemies(target, strikeRadius, damage, bullet.hitLayerMask);
        
        return true;
    }


    Collider2D[] FindTarget(Vector2 pos, LayerMask layer)
    {   
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, searchRadius, layer);

        // Enemy nearestEnemy = null;
        // OreStone nearestOre = null;
        // float enemyDistSq = float.MaxValue;
        // float oreDistSq = float.MaxValue;
        return cols.OrderBy(i => Random.value).Take(strikeCount).ToArray();
    }

}
