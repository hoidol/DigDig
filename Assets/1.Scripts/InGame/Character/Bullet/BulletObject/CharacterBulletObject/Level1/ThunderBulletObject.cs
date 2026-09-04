using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ThunderBulletObject : AllyBulletObject
{
    ThunderBulletSpec thunderBullet;
    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet,allyUnit);
        thunderBullet = bullet as ThunderBulletSpec;
    }


    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable result = base.Hit(hit2D);
        
        Collider2D[] targets = FindTarget(hit2D.point,  thunderBullet.hitLayerMask);
        
        DamageData damageData = new DamageData();
        damageData.damage = thunderBullet.damage;
        for(int i = 0; i < targets.Length; i++)
        {
            EffectManager.Instance.Play(EffectType.Spark, targets[i].transform.position);    
            targets[i].GetComponent<IHittable>().TakeDamage(damageData);
        }
        return result;
    }



    Collider2D[] FindTarget(Vector2 pos, LayerMask layer)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, thunderBullet.searchRadius, layer);
        // 피뢰침 표식(LightningRodMark)이 있는 적 우선 타격
        return cols            
            .OrderBy(_ => Random.value)
            .Take(thunderBullet.strikeCount)
            .ToArray();
    }
}

