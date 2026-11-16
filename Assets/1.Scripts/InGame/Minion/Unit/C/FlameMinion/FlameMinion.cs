using UnityEngine;

// 스킬(2026-11-15): SKILL_COOLTIME초마다 공격 사거리 안 적 전부에게 화상 (쿨타임이 차면 적이 없어도 바로 발동)
// [각성] (진화) EVOLVE_SKILL_COOLTIME초마다 공격 사거리 안 적 최대 EVOLVE_SKILL_MAX_HIT기에 공격력 피해 + 화상
public class FlameMinion : MinionGrowth1
{
    public const float SKILL_COOLTIME = 8f;
    const int SKILL_MAX_TARGETS = 100; // "주변 전체" (FindTargets가 개수만큼 공간을 미리 잡아서 상한)
    float flameSkillTimer;
    public override float SkillProgress => Mathf.Clamp01(flameSkillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { flameSkillTimer = SKILL_COOLTIME; }
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
        flameSkillTimer = 0;

        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        // 탄 피해량은 스펙 damage에서 가져옴 (Shoot의 damage 인자는 탄 피해에 안 쓰임) → 안 넣으면 0이라 최소 피해 1만 들어감
        flameBulletSpec.damage = AttackPower;
        flameBulletSpec.burnDuration = burnDuration;
        flameBulletSpec.burnDPS = burnDPS;
        flameBulletSpec.StartBulletSpec();
        return flameBulletSpec.Instantiate(this);
    }

    public override void Update()
    {
        base.Update();
        UpdateFlameSkill();
        if (!isEvolved)
            return;
        skillTimer += Time.deltaTime;
        if (skillTimer < EVOLVE_SKILL_COOLTIME)
            return;
        skillTimer = 0;
        ExecuteEvolveSkill();
    }

    void UpdateFlameSkill()
    {
        if (flameSkillTimer < SKILL_COOLTIME)
        {
            flameSkillTimer += Time.deltaTime;
            return;
        }
        // 쿨타임이 차면 적이 없어도 바로 발동 (대기 없음)
        flameSkillTimer = 0;
        PlaySkillAnim();
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.Flame, transform.position);
        var targets = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, SKILL_MAX_TARGETS);
        if (targets == null)
            return;
        foreach (Transform target in targets)
        {
            if (target != null && target.TryGetComponent(out StatusEffectHandler handler))
                handler.Apply(new FlameEffect(burnDuration, burnDPS));
        }
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
