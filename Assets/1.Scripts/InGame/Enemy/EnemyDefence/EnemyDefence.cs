using UnityEngine;

// 방어 타입별 피해 배율. 적 프리팹에 붙여서 사용 (Enemy.TakeDamage에서 호출, 없으면 Enemy.Awake에서 enemyData.defenceType으로 추가)
// 배율 값은 DefenceInfo 표 한 곳에서 관리. 방어형별 특수 처리가 필요하면 하위 클래스에서 추가
public abstract class EnemyDefence : MonoBehaviour
{
    public abstract DefenceType DefenceType { get; }

    Enemy enemy;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    // 최종 피해 배율 반환
    public abstract float CalculateDamage(DamageData damage);

    // 공통 배율: 방어 타입 표 배율(만능 공격은 무시) × 거인 사냥 배율(엘리트/보스)
    protected float GetMultiplier(DamageData damage, DefenceType defenceType)
    {
        // 만능 공격 타일: 방어 타입 배율 무시 (모든 방어 타입에 100%). 카오스(Chaos)는 표 값 사용
        float multiplier = damage.IgnoreDefence ? 1f : DefenceInfo.Get(damage.attackType, defenceType);
        // (2026-11-24) 용사 강화: 약점(순혈)·내성 완화·공격 타입별 방어 타입 상대 피해 (아군 공격만)
        if (damage is AllyUnitDamageData)
            multiplier = HeroUpgradeSystem.ApplyDefenceMultiplier(multiplier, damage.attackType, defenceType);
        // 방어 타입별 피해 +% (종족 버프 등)
        if (damage is AllyUnitDamageData allyDamage)
            multiplier *= allyDamage.GetDefenceDamageMul(defenceType);
        // 거인 사냥 타일: 엘리트/보스 추가 배율
        if (damage is AllyUnitDamageData ad && (enemy is EliteEnemy || enemy is Boss))
            multiplier *= ad.eliteBossDamageMul;
        return multiplier;
    }

    public static System.Type GetDefenceClass(DefenceType defenceType)
    {
        switch (defenceType)
        {
            case DefenceType.Armored: return typeof(ArmoredEnemyDefence);
            case DefenceType.Ethereal: return typeof(EtherealEnemyDefence);
            case DefenceType.Light: return typeof(LightEnemyDefence);
            default: return typeof(NormalEnemyDefence);
        }
    }
}
