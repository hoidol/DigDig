using UnityEngine;
using System.Collections.Generic;

public class PierceBulletObject : AllyBulletObject
{

    public override void Shoot(Vector2 dir,float damage)
    {
        base.Shoot(dir,damage);
        transform.right = dir; 
    }

    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet,allyUnit);
        PierceBulletSpec pierceBullet = bullet as PierceBulletSpec;
        AddBehavior(new PierceBehavior(pierceBullet.pierceCount));
    }
}

