using UnityEngine;
using System.Collections.Generic;

public class IceBulletObject : AllyBulletObject
{
    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet,allyUnit);
        IceBulletSpec iceBullet = bullet as IceBulletSpec;
        AddBehavior(new IceOnHitBehavior(iceBullet.duration));
    }


}

