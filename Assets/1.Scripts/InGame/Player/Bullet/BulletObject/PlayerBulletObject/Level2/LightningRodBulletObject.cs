using UnityEngine;

public class LightningRodBulletObject : PlayerBulletObject
{
    int killCount;
     int lv;
     DamageBoostForce damageBoostForce;
     public override void Shoot(Vector2 dir)
    {
        base.Shoot(dir);

        // lv = Player.Instance.statMgr.bulletStatDic[key].lv;
    }

    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable result = base.Hit(hit2D);
        if (result != null && result.Transform.TryGetComponent<LightningRodMark>(out LightningRodMark lRMrk))
        {
            
        }


        return result;
    }
}