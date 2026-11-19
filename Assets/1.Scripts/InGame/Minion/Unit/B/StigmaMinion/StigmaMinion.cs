using UnityEngine;

// [B] 낙인: 일반 탄 + 적중 시 확률로 낙인 (StigmaEffect, 중첩 없음). 낙인 걸린 적이 받는 모든 피해 증가
// 확률은 머지 레벨(StigmaMinionData.stigmaChances), 피해 증가량은 StigmaMinionData.damageBonus
// 스킬(2026-11-15): SKILL_COOLTIME초마다 공격 사거리 안 적 전부에게 낙인 (확률 없음, 적이 없으면 찬 채로 대기), 발동 시 미니언 위치에 StigmaEffect
// [각성] (진화) 낙인 피해 증가량 EVOLVE_DAMAGE_BONUS
public class StigmaMinion : MinionGrowth1
{
    public const float EVOLVE_DAMAGE_BONUS = 0.25f;
    public const float SKILL_COOLTIME = 10f;
    const int SKILL_MAX_TARGETS = 100; // "주변 전체" (FindTargets 상한)
    float skillTimer;

    public override float SkillProgress => Mathf.Clamp01(skillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { skillTimer = SKILL_COOLTIME; }
    float DamageBonus => isEvolved ? Mathf.Max(damageBonus, EVOLVE_DAMAGE_BONUS) : damageBonus;
    StigmaMinionData stigmaMinionData;
    AllyBulletSpec allyBulletSpec;

    public float stigmaChance;
    public float damageBonus;

    public override void Spawn(Vector2 pos, int lv)
    {
        stigmaMinionData = MinionManager.Instance.GetMinionData(key) as StigmaMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        stigmaChance = stigmaMinionData.stigmaChances.GetByMergeLevel(mergeLevel);
        damageBonus = stigmaMinionData.damageBonus;
        skillTimer = 0;
        base.InitMinion();
    }

    public override void Update()
    {
        base.Update();
        if (skillTimer < SKILL_COOLTIME)
        {
            skillTimer += Time.deltaTime;
            return;
        }
        var targets = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, SKILL_MAX_TARGETS);
        if (targets == null || targets.Count == 0)
            return; // 적이 오면 바로
        skillTimer = 0;
        foreach (Transform target in targets)
        {
            if (target != null && target.TryGetComponent(out Enemy enemy) && enemy.CurHp > 0)
                enemy.ApplyStatusEffect(new StigmaEffect(DamageBonus));
        }
        PlaySkillAnim();
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.Stigma, transform.position);
        SoundManager.Play(SFXType.StigmaSkill);
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        allyBulletSpec.AddBulletBehaviour(new StigmaOnHitBehavior(stigmaChance, DamageBonus));
        return allyBulletSpec.Instantiate(this);
    }
}
