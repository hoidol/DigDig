using UnityEngine;

using System.Collections.Generic;


public class NormalBulletObject : PlayerBulletObject
{
    public override void SetBullet(Bullet bullet)
    {
        base.SetBullet(bullet);
        AddBehavior(new BounceBehavior(Player.Instance.statMgr.Bounce));
    }

}

