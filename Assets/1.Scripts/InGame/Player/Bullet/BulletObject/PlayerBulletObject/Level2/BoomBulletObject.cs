using UnityEngine;
using System.Collections.Generic;

public class  BoomBulletObject : PlayerBulletObject
{
    
    public  LayerMask layer;
    public override void SetBullet(Bullet bullet)
    {
        BoomBullet boomBullet = bullet as BoomBullet;
        damageMultiplier = boomBullet.multiplyAtk;
        
        AddBehavior(new BoomBehaviour(boomBullet.boomRange, damage,layer));
        AddBehavior(new BounceBehavior(Player.Instance.bounce));

    }
}

