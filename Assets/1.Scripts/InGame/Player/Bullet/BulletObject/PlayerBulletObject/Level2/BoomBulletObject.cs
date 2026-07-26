using UnityEngine;
using System.Collections.Generic;

public class BoomBulletObject : PlayerBulletObject
{
    float radius;

    public override void SetBullet(Bullet bullet)
    {
        BoomBullet boomBullet = bullet as BoomBullet;
        radius = boomBullet.boomRange;
        // AddBehavior(new BoomBehaviour(boomBullet.boomRange, damage, hitLayerMask));
        AddBehavior(new BounceBehavior(Mathf.Clamp(Player.Instance.bounce, 0, 2)));
    }
    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable hittable = base.Hit(hit2D);
        if (hittable == null)
            return null;
        float finalDamage = damage * damageMultiplier;
        InGameUtil.DamageEnemies(transform.position, radius, finalDamage, hitLayerMask);
        damageMultiplier *= Player.Instance.statMgr.AmmoEfficiency;
        return null;

    }
}

