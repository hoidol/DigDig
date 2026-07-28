using UnityEngine;

using System.Collections.Generic;


public class NormalBulletObject : PlayerBulletObject
{
    public override void Shoot(Vector2 dir)
    {
        base.Shoot(dir);
    }
    public override void SetBullet(Bullet bullet)
    {
        base.SetBullet(bullet);
        AddBehavior(new BounceBehavior(Player.Instance.statMgr.Bounce));
        Debug.Log($"NormalBulletObject 튕기는 횟수 : {Player.Instance.statMgr.Bounce}");
    }

}

