using UnityEngine;

// [각성] (진화) EVOLVE_SKILL_COOLTIME초마다 공격 사거리 안 적 최대 EVOLVE_SKILL_MAX_HIT기에 공격력 피해 + 화상
public class FlameMinion : MinionGrowth1
{
    public const float EVOLVE_SKILL_COOLTIME = 5f;
    public const int EVOLVE_SKILL_MAX_HIT = 7;

    FlameBulletSpec flameBulletSpec;
    public FlameMinionData flameMinionData;
    public float burnDuration;
    public float burnDPS;
    float skillTimer;
    readonly AllyUnitDamageData skillDamageData = new AllyUnitDamageData();

    public override void Spawn(Vector2 pos, int lv)
    {
        flameMinionData = MinionManager.Instance.GetMinionData(key) as FlameMinionData;
        base.Spawn(pos,lv);

        flameBulletSpec = new FlameBulletSpec();
    }

    public override void InitMinion()
    {
        burnDuration = flameMinionData.burnDuration;
        burnDPS = flameMinionData.burnDPSES[mergeLevel];
        skillTimer = 0;

        base.InitMinion();
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
        if (!isEvolved)
            return;
        skillTimer += Time.deltaTime;
        if (skillTimer < EVOLVE_SKILL_COOLTIME)
            return;
        skillTimer = 0;
        ExecuteEvolveSkill();
    }

    void ExecuteEvolveSkill()
    {
        foreach (Transform target in InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, EVOLVE_SKILL_MAX_HIT))
        {
            if (target == null || !target.TryGetComponent(out IHittable hittable))
                continue;
            if (target.TryGetComponent(out StatusEffectHandler handler))
                handler.Apply(new FlameEffect(burnDuration, burnDPS));
            skillDamageData.Init(this);
            skillDamageData.damage = AttackPower;
            skillDamageData.attackType = AttackType;
            AccumulateDamage(skillDamageData.damage);
            hittable.TakeDamage(skillDamageData);
        }
    }
}
