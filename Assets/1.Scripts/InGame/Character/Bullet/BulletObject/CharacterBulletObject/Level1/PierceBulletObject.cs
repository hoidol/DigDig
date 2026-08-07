using UnityEngine;
using System.Collections.Generic;

public class PierceBulletObject : CharacterBulletObject
{

    public override void Shoot(Vector2 dir)
    {
        base.Shoot(dir);
        transform.right = dir; ;
    }

    public override void SetBullet(Bullet bullet)
    {
        base.SetBullet(bullet);
        PierceBullet pierceBullet = bullet as PierceBullet;
        // Debug.Log($"PierceBulletObject pierceBullet.pierceCount {pierceBullet.pierceCount}");
        AddBehavior(new PierceBehavior(pierceBullet.pierceCount));
    }
}

