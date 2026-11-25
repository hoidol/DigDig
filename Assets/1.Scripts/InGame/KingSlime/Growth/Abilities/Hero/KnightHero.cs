using System.Collections.Generic;
using UnityEngine;

// (2026-11-23) 인간 기사: 정예병 칸 미니언 공격력 +10%. 에너지 = 투지
// (2026-11-25) 개편
//   기본 공격 = 근거리 범위 공격(칼 휘두르기): 타겟 방향 부채꼴(swingAngle도, 반경 = 킹 공격 범위) 안 적 전부 킹 공격력 × swingDamageMul (치명타 적용, 탄 없음)
//   1단계 "칼날 회오리" = spinDuration초 동안 킹 주변(spinRadius)을 칼이 빠르게 돌며 닿은 적에게 킹 공격력 × spinDamageMul (같은 적 spinHitInterval초마다) + 둔화
//   2단계 "심판의 대검" (위치 지정) = 누른 곳에 대검이 떨어져 반경 greatswordRadius 안 적에게 킹 공격력 × greatswordDamageMul + greatswordStun초 스턴 (엘리트·보스는 스턴 절반)
public class KnightHero : HeroKingAbility
{
    [Header("기본 공격: 칼 휘두르기")]
    public float swingAngle = 150f;
    public float swingDamageMul = 1f;
    public Color swingColor = new Color(1f, 0.95f, 0.75f, 0.9f);

    [Header("1단계: 칼날 회오리")]
    public float spinDuration = 5f;
    public float spinRadius = 1.6f;
    public float spinDamageMul = 1f;
    public float spinHitInterval = 0.5f;
    public float spinSlowTime = 3f;
    [Tooltip("둔화 = 이동속도 × 이 값 (0.5 = 50% 감소, 중첩 없음, 발밑 둔화 표시)")]
    public float spinSlowRate = 0.5f;
    public float spinRotateSpeed = 900f; // 도/초 (그림)
    public Sprite swordSprite;

    [Header("2단계: 심판의 대검")]
    public float greatswordRadius = 1.5f;
    public float greatswordDamageMul = 20f;
    public float greatswordStun = 5f;
    public float greatswordFallTime = 0.3f;

    public bool IsSpinActive => spin != null;
    HeroSpinBlade spin;

    protected override string GetAttackFormText() => "근거리 범위 공격 (칼 휘두르기)";

    // (2026-11-25) 수치 = HeroData.csv (Skill1Value: 지속 시간, 피해 / Skill2Value: 피해, 스턴), 스킬 강화 카드 = 강화 값
    float SpinDuration => SkillValue(0, 0, spinDuration);
    float SpinDamage => SkillValue(0, 1, spinDamageMul);
    float GreatswordDamage => SkillValue(1, 0, greatswordDamageMul);
    float GreatswordStun => SkillValue(1, 1, greatswordStun);
    float GreatswordRadius => greatswordRadius;

    protected override string GetSkillText(int idx) => idx == 0
        ? $"{Num(SpinDuration)}초간 킹 주변을 칼이 빠르게 돌며 닿은 적에게 킹 공격력 {Num(SpinDamage * 100f)}% 피해 + 둔화"
        : $"누른 곳에 대검이 떨어져 킹 공격력 {Num(GreatswordDamage * 100f)}% 범위 피해 + {Num(GreatswordStun)}초 스턴";

    public override bool NeedsTargetPosition(int idx) => idx == 1;
    public override float GetTargetRadius(int idx) => idx == 1 ? GreatswordRadius : 0f;

    // 기본 공격: 부채꼴 휘두르기. (2026-11-25) 킹 칸 룬 적용 (다중 타겟 제외: 휘두르기 한 번이 범위 공격), 연발 = 휘두르기 반복
    public override bool TryKingAttack(Vector2 dir)
    {
        if (king == null || king.statMgr == null)
            return false;
        Swing(dir);
        RunRuneBurst(() => Swing(dir));
        return true;
    }

    void Swing(Vector2 dir)
    {
        Vector2 center = king.transform.position;
        float radius = king.statMgr.AttackRange + 0.3f;
        float cos = Mathf.Cos(swingAngle * 0.5f * Mathf.Deg2Rad);
        foreach (Enemy e in EnemiesInRange(center, radius))
        {
            Vector2 to = (Vector2)e.transform.position - center;
            if (to.sqrMagnitude > 0.0001f && Vector2.Dot(to.normalized, dir) < cos)
                continue;
            DamageEnemy(e, KingAttackPower * swingDamageMul, true, true, dir);
            Fx(EffectType.Hit, e.transform.position);
        }
        HeroSkillVisual.PlaySwing(center, dir, radius, swingAngle, swingColor);
    }

    protected override bool CastSkill(int idx)
    {
        if (idx != 0)
            return false;
        if (AliveEnemies().Count == 0)
            return false;
        if (spin != null)
            Destroy(spin.gameObject);
        Fx(EffectType.SmallExplosion, king.transform.position);
        spin = HeroSpinBlade.Spawn(this, SpinDuration, spinRadius, spinHitInterval, spinRotateSpeed, swordSprite);
        return true;
    }

    protected override bool CastSkillAt(int idx, Vector2 pos)
    {
        if (idx != 1)
            return CastSkill(idx);
        HeroGreatSwordDrop.Spawn(this, pos, GreatswordRadius, greatswordFallTime, swordSprite);
        return true;
    }

    // 칼날 회오리 1회 타격 (HeroSpinBlade)
    public void SpinHit(Enemy e)
    {
        DamageEnemy(e, KingAttackPower * SpinDamage, false, false, default, TAG_SKILL1);
        Fx(EffectType.Hit, e.transform.position);
        ApplyStatus(e, new FixedSlowEffect(spinSlowTime, spinSlowRate));
    }

    // 대검 착탄 (HeroGreatSwordDrop)
    public void GreatswordImpact(Vector2 pos)
    {
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.MortarExplosion, pos);
        Fx(EffectType.Explosion, pos);
        foreach (Enemy e in DamageArea(pos, GreatswordRadius, KingAttackPower * GreatswordDamage, TAG_SKILL2))
            ApplyStatus(e, new StunEffect(GreatswordStun));
    }

    protected override void OnHeroUnequip()
    {
        if (spin != null)
            Destroy(spin.gameObject);
        spin = null;
    }
}
