using System.Collections.Generic;
using UnityEngine;

public class BounceSlime : SlimeGrowth1
{
    BounceBulletSpec bounceBulletSpec;
    public int bounceCount ;
    public BounceSlimeData  bounceSlimeData;
    public BounceEnhance8Ability damageBoostAbility;
    public List<IBulletForce> bulletForces = new List<IBulletForce>();
    public override void Spawn(Vector2 pos, int lv)
    {
        bulletForces.Clear();
        bounceSlimeData = SlimeManager.Instance.GetSlimeData(key) as BounceSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        bounceCount = bounceSlimeData.bounces[mergeLevel];
        bounceBulletSpec = new BounceBulletSpec();
        
        base.InitSlime();
    }

    public override AllyBulletObject GetBullet()
    {
        bounceBulletSpec.bounce = bounceCount;
        bounceBulletSpec.damage = AttackPower;
        bounceBulletSpec.StartBulletSpec();

        if (damageBoostAbility.isActivate)
        {
             bounceBulletSpec.AddBulletForce(new DamageBoostForce(damageBoostAbility.boostForceValue)); 
        }
        
        
        return bounceBulletSpec.Instantiate(this);
    }


}