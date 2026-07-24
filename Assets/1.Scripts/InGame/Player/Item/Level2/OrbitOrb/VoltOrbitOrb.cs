using UnityEngine;
using System.Collections.Generic;


// 회전 오브젝트 베이스: 회전은 OrbitItemBase의 컨테이너가 담당, 여기선 피해 처리만
public class VoltOrbitOrb : OrbitOrb
{
    public float voltDamage;
    public float voltChance;
    public float voltRadius;
    public LayerMask hittableLayer;
    public override void OnHit(Collider2D other, IHittable hittable)
    {
        base.OnHit(other, hittable);
        //주변에 번개 떨어드리기
        if(Random.value <= voltChance)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            Strike(hitPoint + Random.insideUnitCircle*1.5f);    
        }
    }

    void Strike(Vector2 pos)
    {
        InGameUtil.DamageEnemies(pos, voltRadius, voltDamage, hittableLayer);
        EffectManager.Instance.Play(EffectType.Spark, pos);
    }
}

