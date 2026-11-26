using UnityEngine;

// 스킬(2026-11-15): SKILL_COOLTIME초마다 공격 사거리 안 적 전부에게 화상 (쿨타임이 차면 적이 없어도 바로 발동)
// (2026-11-26) 진화 = MinionEvolutionSetting + EvolutionMinion/Behaviours (예전 [각성] 효과 삭제)
public class FlameMinion : Minion
{
    public const float SKILL_COOLTIME = 8f;
    const int SKILL_MAX_TARGETS = 100; // "주변 전체" (FindTargets가 개수만큼 공간을 미리 잡아서 상한)
    float flameSkillTimer;
    public override float SkillProgress => Mathf.Clamp01(flameSkillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { flameSkillTimer = SKILL_COOLTIME; }

    FlameBulletSpec flameBulletSpec;
    public FlameMinionData flameMinionData;
    public float burnDuration;
    public float burnDPS;

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
        if (IsBeingDragged) // 드래그 중엔 스킬 발동 안 함
            return;
        UpdateFlameSkill();
    }

    void UpdateFlameSkill()
    {
        if (flameSkillTimer < SKILL_COOLTIME)
        {
            flameSkillTimer += SkillDeltaTime;
            return;
        }
        // 쿨타임이 차면 적이 없어도 바로 발동 (대기 없음)
        flameSkillTimer = 0;
        PlaySkillAnim();
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.Flame, transform.position);
        SoundManager.Play(SFXType.FlameSkill);
        var targets = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, SKILL_MAX_TARGETS);
        if (targets == null)
            return;
        foreach (Transform target in targets)
        {
            if (target != null && target.TryGetComponent(out StatusEffectHandler handler))
                handler.Apply(new FlameEffect(burnDuration, burnDPS, this));
        }
    }
}
