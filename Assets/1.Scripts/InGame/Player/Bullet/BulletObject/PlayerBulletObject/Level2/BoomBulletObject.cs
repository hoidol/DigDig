using UnityEngine;
using System.Collections.Generic;

public class BoomBulletObject : PlayerBulletObject
{
    float radius;

    public override void SetBullet(Bullet bullet)
    {
        base.SetBullet(bullet);
        BoomBullet boomBullet = bullet as BoomBullet;
        radius = boomBullet.boomRange;
        damage = Player.Instance.statMgr.AttackPower;
        AddBehavior(new BounceBehavior(Mathf.Clamp(Player.Instance.statMgr.Bounce, 0, 2)));
    }
    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable hit = hit2D.collider.GetComponent<IHittable>();
        if (hit == null)
            return null;

        if (preTarget == hit)
            return null;

        preTarget = hit;

        float finalDamage = damage * damageMultiplier;

        if (finalDamage < 1f)
            finalDamage = 1f;

        InGameUtil.DamageEnemies(transform.position, radius, finalDamage, hitLayerMask);


        bool shouldRelease = true;
        foreach (var b in behaviors)
        {
            shouldRelease = b.OnHit(this, hit, hit2D, direction); //입사 벡터, 법선 벡터, 전달 필요 
            if (!shouldRelease)
                break;
        }

        if (shouldRelease)
        {
            Debug.Log("PlayerBUlletObject Hit ShouldRelease");
            Release();
        }
        return hit;


    }
}

