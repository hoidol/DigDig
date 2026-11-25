using System.Collections.Generic;

// 미니언이 적에게 준 피해를 종(key)별로 한 판 동안 합산 (킹 피해는 화면 집계에 안 넣음)
// (2026-11-25) 킹 피해는 따로 구분별로 합산 (KingDamage: 기본 공격 / 용사 1·2단계 스킬, AllyUnitDamageData.meterTag) → 용사 피해 측정·테스트용
// 집계 = Enemy.TakeDamage 한 곳(실제로 줄어든 체력), 출처 = DamageData.Attacker. 초기화 = GameManager.StartGame
// 피해마다 호출되므로 이벤트 발행 없이 값만 더하고, UI(DamageMeterSystemContainer)가 주기적으로 읽는다
public static class DamageMeterSystem
{
    static readonly Dictionary<string, double> damageDic = new();

    public static double TotalDamage { get; private set; }

    static readonly Dictionary<string, double> kingDamageDic = new();
    public static double KingTotalDamage { get; private set; }

    public static void Record(DamageData data, float damage)
    {
        if (damage <= 0 || data == null)
            return;
        if (data.Attacker is KingSlime)
        {
            string tag = data is AllyUnitDamageData a && !string.IsNullOrEmpty(a.meterTag) ? a.meterTag : HeroKingAbility.TAG_BASIC;
            kingDamageDic.TryGetValue(tag, out double v);
            kingDamageDic[tag] = v + damage;
            KingTotalDamage += damage;
            return;
        }
        Record(data.Attacker, damage);
    }

    public static double GetKingDamage(string tag) => tag != null && kingDamageDic.TryGetValue(tag, out double v) ? v : 0;

    public static void Record(IAllyUnit attacker, float damage)
    {
        if (damage <= 0 || attacker is not Minion minion || minion.minionData == null)
            return;

        string key = minion.minionData.key;
        damageDic.TryGetValue(key, out double value);
        damageDic[key] = value + damage;
        TotalDamage += damage;
    }

    public static double GetDamage(string key)
    {
        return key != null && damageDic.TryGetValue(key, out double value) ? value : 0;
    }

    public static void Clear()
    {
        damageDic.Clear();
        TotalDamage = 0;
        kingDamageDic.Clear();
        KingTotalDamage = 0;
    }
}
