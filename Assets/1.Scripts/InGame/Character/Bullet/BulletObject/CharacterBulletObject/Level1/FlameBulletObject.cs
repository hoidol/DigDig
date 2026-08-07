using UnityEngine;
using System.Collections.Generic;

public class FlameBulletObject : CharacterBulletObject
{

    public override void Shoot(Vector2 dir)
    {
        base.Shoot(dir);
    }

    public override void SetBullet(Bullet bullet)
    {
        base.SetBullet(bullet);
        FlameBullet flameBullet = bullet as FlameBullet;

        AddBehavior(new FlameOnHitBehavior(flameBullet.burnDuration, flameBullet.burnDPS));
    }
}

