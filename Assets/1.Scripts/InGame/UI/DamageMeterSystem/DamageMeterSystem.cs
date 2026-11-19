using System.Collections.Generic;

// 미니언이 적에게 준 피해를 종(key)별로 한 판 동안 합산 (킹 피해는 세지 않음)
// 집계 = Enemy.TakeDamage 한 곳(실제로 줄어든 체력), 출처 = DamageData.Attacker. 초기화 = GameManager.StartGame
// 피해마다 호출되므로 이벤트 발행 없이 값만 더하고, UI(DamageMeterSystemContainer)가 주기적으로 읽는다
public static class DamageMeterSystem
{
    static readonly Dictionary<string, double> damageDic = new();

    public static double TotalDamage { get; private set; }

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
    }
}
