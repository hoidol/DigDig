using System.Collections.Generic;
using UnityEngine;

public class IceSlime : SlimeGrowth1
{
    IceBulletSpec iceBulletSpec;
    IceSlimeData iceSlimeData;
    public float iceDuration;
    // public float effectChance;
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

        iceDuration = iceSlimeData.durations.GetByMergeLevel(mergeLevel);
        // effectChance = iceSlimeData.GetUniqueStat(SlimeStatType.EffectChance).GetValue<float>(mergeLevel);
        base.InitSlime();
    }

    public override AllyBulletObject GetBullet()
    {
        iceBulletSpec.damage = AttackPower;
        iceBulletSpec.duration = iceDuration;
        iceBulletSpec.chance = 1;

        iceBulletSpec.StartBulletSpec();
        if (bounceAbility.isActivate)
        {
            iceBulletSpec.AddBulletBehaviour(new BounceBehavior(bounceAbility.bounceCount, 1));

        }
        return iceBulletSpec.Instantiate(this);
    }


    public override int ShootCount => iceSlimeData.shootCounts.GetByMergeLevel(mergeLevel);

    public override List<Transform> FindTargets(int count)
    {
        // 타겟 탐색 능력: 이미 얼어있는 적은 후순위
        string exceptEffect = targetSearcher.isActivate ? "Ice" : null;
        return InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, count, exceptEffect);
    }

    public override Transform FindTarget()
    {
        if (!targetSearcher.isActivate)
        {
            return base.FindTarget();
        }
        else
        {
            return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask, findTargetType, "Ice");
        }

    }

}