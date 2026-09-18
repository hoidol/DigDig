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
        burnDPS = float.Parse(flameSlimeData.GetUniqueStat(SlimeStatType.FlameDPS).values[mergeLevel]);

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


    public override Transform FindTarget()
    {
        if (!targetSearcher.isActivate)
        {
            return base.FindTarget();
        }
        else
        {
            return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask, "Flame");
        }

    }

}