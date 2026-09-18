using UnityEngine;

public class IceSlime : SlimeGrowth1
{
    IceBulletSpec iceBulletSpec;
    IceSlimeData iceSlimeData;
    public float iceDuration;
    public float effectChance;
    public IceEnhance8Ability bounceAbility;//각성 능력 
    public IceEnhance11Ability targetSearcher; //타겟 탐색

    public override void Spawn(Vector2 pos, int lv)
    {
        iceSlimeData = SlimeManager.Instance.GetSlimeData(key) as IceSlimeData;
        base.Spawn(pos, lv);
    }
    

    public override void InitSlime()
    {
        iceBulletSpec = new IceBulletSpec();
        
        iceDuration = iceSlimeData.durations[mergeLevel];
        effectChance = float.Parse(iceSlimeData.GetUniqueStat(SlimeStatType.EffectChance).values[mergeLevel]);
        base.InitSlime();
    }
    
    public override AllyBulletObject GetBullet()
    {
        iceBulletSpec.damage= AttackPower;
        iceBulletSpec.duration = iceDuration;
        iceBulletSpec.chance = effectChance;        
    
        iceBulletSpec.StartBulletSpec();
        if (bounceAbility.isActivate)
        {
            iceBulletSpec.AddBulletBehaviour(new BounceBehavior(bounceAbility.bounceCount));
            
        }
        return iceBulletSpec.Instantiate(this);
    }


    public override Transform FindTarget()
    {
        if (!targetSearcher.isActivate)
        {
            return base.FindTarget();
        }
        else
        {
            return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask, "Ice");
        }
        
    }

}