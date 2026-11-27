using UnityEngine;

// (2026-11-23) 레인저: 정예병 칸 미니언 탄 20% 확률로 2발(DoubleShotChance). 에너지 = 집중
// (2026-11-25) 개편
//   기본 공격 = 물리 · 다중 타겟 2 (1 + bonusTargets)
//   1단계 "화살비" (위치 지정) = 누른 곳(rainRadius)에 rainDuration초 동안 화살비, 1초마다 슬라임 공격력 × rainDamageMul 범위 피해 (발동 즉시 1번 → rainDuration번)
//   2단계 "매의 눈" = eyeDuration초 동안 필드의 모든 미니언·슬라임 공격 범위 ×eyeRangeMul + 모두 체력이 가장 높은 적을 노림
//   (2026-11-26 사용자 변경: 공속 증가 삭제, 공격 범위 +50% / 강화 +100% = HeroData.csv Skill2Value 0.5/1)
public class RangerHero : HeroSlimeAbility
{
    public int bonusTargets = 1;

    [Header("1단계: 화살비")]
    public float rainDuration = 5f;
    public float rainRadius = 1.5f;
    public float rainDamageMul = 5f;
    public float rainTickInterval = 1f;

    [Header("2단계: 매의 눈")]
    public float eyeDuration = 4f;
    public float eyeRangeMul = 1.5f; // HeroData.csv Skill2Value가 없을 때만

    int addedTargets;
    readonly UnitBuffGroup eyeBuff = new UnitBuffGroup();
    public bool IsEyeActive => eyeBuff.IsActive;

    protected override string GetAttackFormText() => $"다중 타겟 {1 + bonusTargets}";

    // (2026-11-25) 수치 = HeroData.csv (Skill1Value: 지속 시간, 피해 / Skill2Value: 지속 시간, 공격 범위 증가 비율(0.5 = +50%)), 스킬 강화 카드 = 강화 값
    float RainDuration => SkillValue(0, 0, rainDuration);
    float RainDamage => SkillValue(0, 1, rainDamageMul);
    float EyeDuration => SkillValue(1, 0, eyeDuration);
    public float EyeRangeMul => 1f + SkillValue(1, 1, eyeRangeMul - 1f);

    protected override string GetSkillText(int idx) => idx == 0
        ? $"누른 곳에 {Num(RainDuration)}초간 화살비, 1초마다 슬라임 공격력 {Num(RainDamage * 100f)}% 범위 피해"
        : $"{Num(EyeDuration)}초간 모든 미니언·슬라임 공격 범위 +{Num((EyeRangeMul - 1f) * 100f)}%, 체력이 가장 높은 적을 공격";

    public override bool NeedsTargetPosition(int idx) => idx == 0;
    public override float GetTargetRadius(int idx) => idx == 0 ? rainRadius : 0f;

    protected override void OnHeroEquip()
    {
        slime.shotModifier.bonusShootCount += bonusTargets;
        addedTargets = bonusTargets;
    }

    protected override void OnHeroUnequip()
    {
        slime.shotModifier.bonusShootCount -= addedTargets;
        addedTargets = 0;
        EndEye();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        eyeBuff.Remove();
    }

    protected override bool CastSkill(int idx)
    {
        if (idx == 0)
            return false; // 위치 지정 (CastSkillAt)
        eyeBuff.Apply(slime, EyeDuration, () => new[]
        {
            new Buff(StatType.AttackRange, EyeRangeMul, StatOpType.Multiply),
        });
        ForcedAllyFindTarget = FindTargetType.HighHp;
        Fx(EffectType.Thunder, slime.transform.position);
        return true;
    }

    protected override bool CastSkillAt(int idx, Vector2 pos)
    {
        if (idx != 0)
            return CastSkill(idx);
        HeroArrowRain.Spawn(this, pos, rainRadius, RainDuration, rainTickInterval);
        return true;
    }

    // 화살비 1회 (HeroArrowRain)
    public void RainTick(Vector2 pos)
    {
        for (int i = 0; i < 3; i++) // 임시 이펙트: 범위 안 랜덤 3곳 + 맞은 적
            Fx(EffectType.Hit, pos + Random.insideUnitCircle * rainRadius);
        foreach (Enemy e in DamageArea(pos, rainRadius, SlimeAttackPower * RainDamage, TAG_SKILL1))
            Fx(EffectType.Hit, e.transform.position);
    }

    protected override void Update()
    {
        base.Update();
        if (eyeBuff.Tick(Time.deltaTime))
            EndEye();
    }

    void EndEye()
    {
        eyeBuff.Remove();
        if (ForcedAllyFindTarget == FindTargetType.HighHp)
            ForcedAllyFindTarget = null;
    }
}
