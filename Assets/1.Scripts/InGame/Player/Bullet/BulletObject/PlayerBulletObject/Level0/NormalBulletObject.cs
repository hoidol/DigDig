using UnityEngine;

using System.Collections.Generic;


public class  NormalBulletObject : PlayerBulletObject
{
    public override void Shoot(Vector2 dir)
    {
        base.Shoot(dir);
    }
    public override void SetBullet(Bullet bullet)
    {        
        AddBehavior(new BounceBehavior(Player.Instance.bounce));
    }

}

