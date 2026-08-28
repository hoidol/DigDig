using UnityEngine;
using System.Collections.Generic;

public class FlameBulletObject : AllyBulletObject
{
    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet,allyUnit);
        FlameBulletSpec flameBullet = bullet as FlameBulletSpec;
        AddBehavior(new FlameOnHitBehavior(flameBullet.burnDuration, flameBullet.burnDPS));
    }


}

