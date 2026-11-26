using System.Collections.Generic;
using UnityEngine;

// (2026-11-23) 네크로멘서: 정예병 칸 미니언 치명타 +5%. 에너지 = 어둠
// (2026-11-25) 개편
//   기본 공격 = 카오스 폭발탄 (HeroSplashProjectile): 타겟에게 날아가 폭발, 반경 splashRadius 안 적 전부 킹 공격력 × splashDamageMul (치명타·룬 적용)
//   1단계 "망령 소환" = ghostDuration초 동안 적 경로를 거꾸로 도는 망령(HeroGhost), 닿은 적에게 킹 공격력 × ghostDamageMul (같은 적 hitInterval초마다)
//   (2026-11-26) 개편: 피해 2배(200%/강화 400%) + 닿은 적 ghostSlowTime초 이동속도 ghostSlowRate 둔화 (FixedSlowEffect, 중첩 없음)
//   2단계 "저주" = 필드의 모든 적 curseDuration초 동안 받는 피해 +curseDamageBonus(CurseEffect) + curseStun초 스턴 (엘리트·보스도 시간 그대로)
public class NecromancerHero : HeroKingAbility
{
    [Header("기본 공격: 카오스 폭발탄 (스플래쉬)")]
    public float splashDamageMul = 1f;
    public float splashRadius = 1.5f;
    public float splashSpeed = 8f;
    [Tooltip("최대 날아가는 거리 = 킹 공격 범위 + 이 값")]
    public float splashExtraDistance = 1f;
    public Color splashColor = new Color(0.75f, 0.45f, 1f, 0.95f);

    [Header("1단계: 망령 소환")]
    public float ghostDuration = 10f;
    public float ghostDamageMul = 2f;
    public float ghostSlowTime = 3f;
    [Tooltip("이동속도 배율 (0.5 = 50% 감소)")]
    public float ghostSlowRate = 0.5f;
    public float ghostSpeed = 2.5f;
    public float ghostRadius = 0.6f;
    public float hitInterval = 0.5f;

    [Header("2단계: 저주")]
    public float curseDuration = 5f;
    public float curseDamageBonus = 0.5f;
    public float curseStun = 3f;

    protected override string GetAttackFormText() => $"카오스 폭발탄 (범위 {Num(splashRadius)})";

    // (2026-11-25) 수치 = HeroData.csv (Skill1Value: 지속 시간, 피해, 둔화 시간(2026-11-26) / Skill2Value: 지속 시간, 받는 피해 증가 비율, 스턴), 스킬 강화 카드 = 강화 값
    float GhostDuration => SkillValue(0, 0, ghostDuration);
    float GhostDamage => SkillValue(0, 1, ghostDamageMul);
    float GhostSlowTime => SkillValue(0, 2, ghostSlowTime);
    float GhostSlowRate => ghostSlowRate; // 프리팹 값
    float CurseDuration => SkillValue(1, 0, curseDuration);
    float CurseDamageBonus => SkillValue(1, 1, curseDamageBonus);
    float CurseStun => SkillValue(1, 2, curseStun);

    protected override string GetSkillText(int idx) => idx == 0
        ? $"{Num(GhostDuration)}초간 망령이 적 경로를 거슬러 돌며 닿은 적에게 킹 공격력 {Num(GhostDamage * 100f)}% 피해 + {Num(GhostSlowTime)}초간 이동속도 {Num((1f - GhostSlowRate) * 100f)}% 감소"
        : $"{Num(CurseDuration)}초간 모든 적이 받는 피해 +{Num(CurseDamageBonus * 100f)}% + {Num(CurseStun)}초 스턴 (엘리트·보스 포함)";

    // (2026-11-25) 기본 공격 = 카오스 폭발탄 (탄 취급: 킹 칸 룬 전부 적용 — 다중 타겟 = 서로 다른 적에게 한 발씩, 연발 = 다시 발사, 적중 효과는 폭발에 맞은 적마다)
    public override bool TryKingAttack(Vector2 dir)
    {
        if (king == null || king.statMgr == null)
            return false;
        List<Vector2> dirs = GetMultiTargetDirs(dir);
        List<Transform> targets = GetMultiTargets(dirs.Count);
        Fire(dirs, targets);
        RunRuneBurst(() => Fire(dirs, targets));
        return true;
    }

    void Fire(List<Vector2> dirs, List<Transform> targets)
    {
        float distance = king.statMgr.AttackRange + splashExtraDistance;
        Vector2 from = king.attackPoint != null ? (Vector2)king.attackPoint.position : (Vector2)king.transform.position;
        for (int i = 0; i < dirs.Count; i++)
            HeroSplashProjectile.Spawn(this, from, i < targets.Count ? targets[i] : null, dirs[i], splashSpeed, distance, splashRadius, splashColor);
    }

    // 폭발 (HeroSplashProjectile): 반경 안 적 전부
    public void SplashExplode(Vector2 pos, float radius, Vector2 dir)
    {
        Fx(EffectType.SmallExplosion, pos);
        HeroSkillVisual.PlayRing(pos, radius, splashColor, 0.25f);
        foreach (Enemy e in EnemiesInRange(pos, radius))
            DamageEnemy(e, KingAttackPower * splashDamageMul, true, true, dir);
    }

    protected override bool CastSkill(int idx)
    {
        if (AliveEnemies().Count == 0)
            return false;
        if (idx == 0)
        {
            Fx(EffectType.Stigma, king.transform.position);
            HeroGhost.Spawn(king, this, GhostDuration, ghostSpeed, ghostRadius, hitInterval);
            return true;
        }
        foreach (Enemy e in AliveEnemies())
        {
            ApplyStatus(e, new CurseEffect(CurseDuration, CurseDamageBonus));
            Fx(EffectType.Stigma, e.transform.position);
            ApplyStatus(e, new StunEffect(CurseStun, true));
        }
        return true;
    }

    // 망령 한 번 타격 (HeroGhost에서 호출)
    public void GhostHit(Enemy enemy)
    {
        DamageEnemy(enemy, KingAttackPower * GhostDamage, false, false, default, TAG_SKILL1);
        if (GhostSlowTime > 0f)
            ApplyStatus(enemy, new FixedSlowEffect(GhostSlowTime, GhostSlowRate));
        Fx(EffectType.Hit, enemy.transform.position);
    }
}
