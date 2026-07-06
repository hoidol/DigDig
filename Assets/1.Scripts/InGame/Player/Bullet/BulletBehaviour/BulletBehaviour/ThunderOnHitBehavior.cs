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
        Collider2D[] targets = FindTarget(hit2D.point, bullet.hitLayerMask);
        float damage = Player.Instance.statMgr.AttackPower * damageRate;
        DamageData damageData = new DamageData();
        damageData.damage = damage;
        for(int i = 0; i < targets.Length; i++)
        {
            EffectManager.Instance.Play(EffectType.Spark, targets[i].transform.position);    
            targets[i].GetComponent<IHittable>().TakeDamage(damageData);
        }
        return true;
    }


    Collider2D[] FindTarget(Vector2 pos, LayerMask layer)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, searchRadius, layer);
        // 피뢰침 표식(LightningRodMark)이 있는 적 우선 타격
        return cols
            .OrderByDescending(c => c.GetComponent<LightningRodMark>() != null ? 1 : 0)
            .ThenBy(_ => Random.value)
            .Take(strikeCount)
            .ToArray();
    }

}
