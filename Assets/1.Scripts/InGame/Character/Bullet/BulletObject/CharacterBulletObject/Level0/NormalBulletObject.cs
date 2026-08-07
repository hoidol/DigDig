using UnityEngine;

using System.Collections.Generic;


public class NormalBulletObject : CharacterBulletObject
{
    public override void SetBullet(Bullet bullet)
    {
        base.SetBullet(bullet);
        AddBehavior(new BounceBehavior(Character.Instance.statMgr.Bounce));
    }

}

