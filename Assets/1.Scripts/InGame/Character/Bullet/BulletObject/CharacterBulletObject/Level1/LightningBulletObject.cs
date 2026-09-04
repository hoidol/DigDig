using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LightningBulletObject : AllyBulletObject
{
    LightningBulletSpec lightningBullet;
    public LineRenderer lineRenderer;
    LightningChainOnHitBehavior lightningChainOnHitBehavior;
    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet,allyUnit);
        lightningBullet = bullet as LightningBulletSpec;
        lightningChainOnHitBehavior = new LightningChainOnHitBehavior(
            lightningBullet.initSearchRadius,
            lightningBullet.searchRadius,
            lightningBullet.lightningCount,
            lightningBullet.damage,
            lightningBullet.hitLayerMask);
    }
    
    public override void Shoot(Vector2 dir,float damage)
    {
        damageData.damage = damage;
        lineRenderer.positionCount = 0;
        lightningChainOnHitBehavior.OnHit(this, transform.position);
    }

    public override void Update()
    {
        
    }

}

