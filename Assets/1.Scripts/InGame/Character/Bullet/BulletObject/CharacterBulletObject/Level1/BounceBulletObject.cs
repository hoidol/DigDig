using UnityEngine;
using System.Collections.Generic;

public class BounceBulletObject : AllyBulletObject
{
    public override void SetBullet(BulletSpec bullet,IAllyUnit allyUnit)
    {
        base.SetBullet(bullet,allyUnit);
        BounceBulletSpec bounceBullet = bullet as BounceBulletSpec;
        AddBehavior(new BounceBehavior(bounceBullet.bounce));
    }
}

