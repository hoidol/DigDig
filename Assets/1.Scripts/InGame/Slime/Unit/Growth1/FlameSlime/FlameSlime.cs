using System.Collections.Generic;
using UnityEngine;

public class FlameSlime : SlimeGrowth1,IExecuteSkillSlime
{
    FlameBulletSpec flameBulletSpec;
    public FlameSlimeData flameSlimeData;
    public float burnDuration;
    public float burnDPS;

    public FlameEnhance8Ability skill1; //각성
    public FlameEnhance11Ability targetSearcher; //타겟 탐색
    float skillTimer;

    public override void Spawn(Vector2 pos, int lv)
    {
        flameSlimeData = SlimeManager.Instance.GetSlimeData(key) as FlameSlimeData;
        base.Spawn(pos,lv);

        flameBulletSpec = new FlameBulletSpec();
    }

    public override void InitSlime()
    {       
        burnDuration = flameSlimeData.burnDuration;
        burnDPS = flameSlimeData.GetUniqueStat(SlimeStatType.FlameDPS).GetValue<float>(mergeLevel);

        base.InitSlime();
    }

    public override AllyBulletObject GetBullet()
    {
        flameBulletSpec.burnDuration = burnDuration;
        flameBulletSpec.burnDPS = burnDPS;
        flameBulletSpec.StartBulletSpec();
        return flameBulletSpec.Instantiate(this);
    }

    public override void Update()
    {
        base.Update();

        if (skill1.isActivate)
        {
            if (skillTimer >= skill1.cooltime)
            {
                ExecuteSkill();
                skillTimer = 0;
            }
            skillTimer += Time.deltaTime ;    
        }
        
    }

    public void ExecuteSkill()
    {
        skill1.Execute();
    }


    public override int ShootCount => flameSlimeData.shootCounts.GetByMergeLevel(mergeLevel);

    public override List<Transform> FindTargets(int count)
    {
        // 타겟 탐색 능력: 이미 불붙은 적은 후순위
        string exceptEffect = targetSearcher.isActivate ? "Flame" : null;
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
            return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask, findTargetType, "Flame");
        }

    }

}