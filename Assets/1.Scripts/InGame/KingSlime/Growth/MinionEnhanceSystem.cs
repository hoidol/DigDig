using System.Collections.Generic;
using UnityEngine;

// (2026-11-25) 레벨업 미니언 강화 카드 (이전 킹 능력치 카드 대체)
// 카드 한 장 = 장착 미니언 한 종 × 스탯 하나 (공격력·공속·공격 범위·치명타 확률·강적 피해(2026-11-25)). 고르면 그 종 전체(필드 + 이후 소환분)에 적용, 같은 카드를 또 고르면 레벨 +1
// 한 판 동안 유지 (GameManager.StartGame에서 초기화). 수치는 STATS 표 (임시)
// 레벨업 창 순서: 단계 카드(용사·스킬 해방·용사 강화) → 미니언 강화 카드 (LevelUpCanvas)
public static class MinionEnhanceSystem
{
    public struct StatInfo
    {
        public StatType statType;
        public float valuePerLevel; // 공격력·공속 = %(곱), 사거리 = 더하기, 치명타 = %p 더하기
        public bool multiply;
        public string name;
        public string suffix;
        public StatInfo(StatType statType, float valuePerLevel, bool multiply, string name, string suffix)
        {
            this.statType = statType;
            this.valuePerLevel = valuePerLevel;
            this.multiply = multiply;
            this.name = name;
            this.suffix = suffix;
        }
    }

    public const int MAX_LEVEL = 5;
    // (2026-11-25) 행운 카드: 카드마다 확률로 한 번에 여러 레벨 (공격력 +15% 카드 → +30%·+45% 카드). 최대 레벨을 넘으면 그보다 낮은 카드
    //   1% = 3레벨(대박), 10% = 2레벨(행운), 나머지 = 1레벨. 3레벨 칸이 안 남으면 2레벨로
    public const float JACKPOT_CHANCE = 1f;
    public const int JACKPOT_AMOUNT = 3;
    public const float LUCKY_CHANCE = 10f;
    public const int LUCKY_AMOUNT = 2;

    // 이 카드가 몇 레벨짜리인지. roll = 0~100 (0~1 = 대박, 1~11 = 행운)
    public static int RollAmount(string key, StatType stat, float roll)
    {
        int room = MAX_LEVEL - GetLevel(key, stat);
        if (roll < JACKPOT_CHANCE)
            return room >= JACKPOT_AMOUNT ? JACKPOT_AMOUNT : room >= LUCKY_AMOUNT ? LUCKY_AMOUNT : 1;
        if (roll < JACKPOT_CHANCE + LUCKY_CHANCE)
            return room >= LUCKY_AMOUNT ? LUCKY_AMOUNT : 1;
        return 1;
    }

    public static readonly StatInfo[] STATS =
    {
        new StatInfo(StatType.AttackPower, 15f, true, "공격력", "%"),
        new StatInfo(StatType.AttackSpeed, 15f, true, "공속", "%"),
        new StatInfo(StatType.AttackRange, 0.3f, false, "공격 범위", ""),
        new StatInfo(StatType.CritChance, 5f, false, "치명타 확률", "%"),
        new StatInfo(StatType.EliteBossDamage, 15f, false, "강적 피해", "%"), // (2026-11-25) 엘리트·보스에게 주는 피해 +% (Add)
    };

    static readonly Dictionary<(string key, StatType stat), int> levels = new Dictionary<(string, StatType), int>();

    // 카드 새로고침 횟수는 RerollCounter.MinionEnhance (무료 2 → 광고 3, 한 판)
    public static void Clear()
    {
        levels.Clear();
        RerollCounter.ResetAll();
    }

    public static int GetLevel(string key, StatType stat) =>
        key != null && levels.TryGetValue((key, stat), out int lv) ? lv : 0;

    public static bool TryGetInfo(StatType stat, out StatInfo info)
    {
        foreach (StatInfo s in STATS)
            if (s.statType == stat)
            {
                info = s;
                return true;
            }
        info = default;
        return false;
    }

    // 레벨 lv일 때 합계 (공격력 3레벨 = 45%)
    public static float GetTotal(StatType stat, int lv) => TryGetInfo(stat, out StatInfo info) ? info.valuePerLevel * lv : 0f;

    // 고르기: 레벨 +amount (행운 카드 = 2) → 그 종 필드 미니언 스탯 다시 계산
    public static void Add(string key, StatType stat, int amount = 1)
    {
        if (string.IsNullOrEmpty(key))
            return;
        SetLevel(key, stat, GetLevel(key, stat) + Mathf.Max(1, amount));
        if (MinionFieldController.Instance != null)
            foreach (Minion m in MinionFieldController.Instance.activeminions)
                if (m != null && m.minionData != null && m.minionData.key == key)
                    m.UpdateMinion();
    }

    // 테스트 복구(WaveSnapshotSystem)용: 지금 강화 레벨 전부 / 통째로 되돌리기
    public static List<(string key, StatType stat, int lv)> GetAllLevels()
    {
        var list = new List<(string, StatType, int)>();
        foreach (var kv in levels)
            list.Add((kv.Key.key, kv.Key.stat, kv.Value));
        return list;
    }

    public static void RestoreLevels(IEnumerable<(string key, StatType stat, int lv)> saved)
    {
        levels.Clear();
        foreach (var s in saved)
            levels[(s.key, s.stat)] = s.lv;
    }

    public static void SetLevel(string key, StatType stat, int lv)
    {
        levels[(key, stat)] = Mathf.Clamp(lv, 0, MAX_LEVEL);
    }

    // Minion.UpdateMinion (버프 적용 전)
    public static void Apply(Minion minion, Dictionary<StatType, float> statDic)
    {
        if (minion != null && minion.minionData != null)
            Apply(minion.minionData.key, statDic);
    }

    public static void Apply(string key, Dictionary<StatType, float> statDic)
    {
        if (levels.Count == 0 || string.IsNullOrEmpty(key))
            return;
        foreach (StatInfo s in STATS)
        {
            int lv = GetLevel(key, s.statType);
            if (lv <= 0)
                continue;
            statDic.TryGetValue(s.statType, out float cur);
            float total = s.valuePerLevel * lv;
            statDic[s.statType] = s.multiply ? cur * (1f + total / 100f) : cur + total;
        }
    }

    // 회전형(Orbit 등)은 타겟을 노리지 않아 공격 범위가 의미 없음 → 범위 카드 제외
    public static bool CanEnhance(MinionData data, StatType stat) =>
        data != null && !(stat == StatType.AttackRange && (data.tags & MinionTag.Orbit) != 0);

    // 카드 후보 = 장착 미니언 × 스탯 (최대 레벨 제외) 중 서로 다른 count장
    public static List<(string key, StatType stat)> PickChoices(IList<string> minionKeys, System.Func<string, MinionData> getData, int count, System.Random rng)
    {
        var pool = new List<(string, StatType)>();
        if (minionKeys != null)
            foreach (string key in minionKeys)
            {
                if (string.IsNullOrEmpty(key) || pool.Exists(p => p.Item1 == key))
                    continue;
                MinionData data = getData?.Invoke(key);
                foreach (StatInfo s in STATS)
                    if (GetLevel(key, s.statType) < MAX_LEVEL && (getData == null || CanEnhance(data, s.statType)))
                        pool.Add((key, s.statType));
            }
        var result = new List<(string, StatType)>();
        while (result.Count < count && pool.Count > 0)
        {
            int i = rng.Next(pool.Count);
            result.Add(pool[i]);
            pool.RemoveAt(i);
        }
        return result;
    }

    // 카드 설명: "공격력 +15%" / 이미 있으면 "공격력 +15% → +30%" (amount = 이 카드로 오르는 레벨, 행운 = 2)
    public static string GetDescription(StatType stat, int nextLevel, int amount = 1)
    {
        if (!TryGetInfo(stat, out StatInfo info))
            return "";
        string Fmt(float v) => (info.suffix == "%" ? v.ToString("0.#") : v.ToString("0.##")) + info.suffix;
        float next = GetTotal(stat, nextLevel);
        int prevLevel = Mathf.Max(0, nextLevel - Mathf.Max(1, amount));
        return prevLevel <= 0 ? $"{info.name} +{Fmt(next)}"
            : $"{info.name} +{Fmt(GetTotal(stat, prevLevel))} → <color=#7CFF6B>+{Fmt(next)}</color>";
    }
}
