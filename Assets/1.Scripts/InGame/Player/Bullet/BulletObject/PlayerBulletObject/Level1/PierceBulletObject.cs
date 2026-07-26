using UnityEngine;
using System.Collections.Generic;

public class PierceBulletObject : PlayerBulletObject
{

    public override void SetBullet(Bullet bullet)
    {
        base.SetBullet(bullet);
        PierceBullet pierceBullet = bullet as PierceBullet;
        // damageMultiplier = pierceBullet.multiplyAtk;

        AddBehavior(new PierceBehavior(pierceBullet.pierceCount));

    }
}

