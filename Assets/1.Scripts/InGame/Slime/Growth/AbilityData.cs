using UnityEngine;

// (2026-11-20) 슬라임 성장 능력 데이터 (레벨업 카드 한 장). prefab의 SlimeAbility 컴포넌트가 실제 동작
// 능력을 바꾸려면 prefab 참조만 바꾸면 됨. 이름·설명은 아직 번역 전이라 에셋에 직접 입력(임시)
[CreateAssetMenu(fileName = "AbilityData", menuName = "Slime/AbilityData")]
public class AbilityData : ScriptableObject
{
    public string key;
    public AbilityCategory category;
    public string title;
    [TextArea]
    [Tooltip("{0}, {1} … = 고르면 될 레벨의 수치 (SlimeAbility.GetDescArgs)")]
    public string desc;
    public Sprite thum;
    [Min(1)] public int maxLevel = 5;
    public SlimeAbility prefab;
    // (2026-11-21) 룬 카드: 고르면 타일을 골라 그 룬을 새김(같은 룬이면 레벨 +1). 실행 중 ItemData로 만듦(CreateRuneCard), 에셋 없음
    [System.NonSerialized] public RuneData runeTile;
    public bool IsRune => runeTile != null;
    // (2026-11-25) 미니언 강화 카드: 장착 미니언 한 종 × 스탯 하나 (MinionEnhanceSystem). 실행 중 만듦(CreateMinionEnhanceCard), 에셋 없음
    [System.NonSerialized] public string minionKey;
    [System.NonSerialized] public StatType minionStat;
    [System.NonSerialized] public int minionAmount = 1; // (2026-11-25) 고르면 오르는 레벨 (행운 카드 = MinionEnhanceSystem.LUCKY_AMOUNT)
    public bool IsMinionEnhance => !string.IsNullOrEmpty(minionKey);
    public bool IsLuckyEnhance => IsMinionEnhance && minionAmount > 1;

    static readonly System.Collections.Generic.Dictionary<(string, StatType, int), AbilityData> minionCards = new System.Collections.Generic.Dictionary<(string, StatType, int), AbilityData>();
    public static AbilityData CreateMinionEnhanceCard(string minionKey, StatType stat, int amount = 1)
    {
        amount = Mathf.Max(1, amount);
        if (minionCards.TryGetValue((minionKey, stat, amount), out AbilityData card) && card != null)
            return card;
        MinionData minion = MinionManager.Instance != null && MinionManager.Instance.minionDataDic.TryGetValue(minionKey, out MinionData d) ? d : null;
        MinionEnhanceSystem.TryGetInfo(stat, out MinionEnhanceSystem.StatInfo info);
        card = CreateInstance<AbilityData>();
        card.name = card.key = $"MinionEnhance_{minionKey}_{stat}";
        card.category = AbilityCategory.MinionEnhance;
        card.title = $"{(minion != null ? minion.Title : minionKey)} {info.name}{(amount > 1 ? $" ×{amount}" : "")}";
        card.thum = minion != null ? minion.thum : null;
        card.maxLevel = MinionEnhanceSystem.MAX_LEVEL;
        card.minionKey = minionKey;
        card.minionStat = stat;
        card.minionAmount = amount;
        minionCards[(minionKey, stat, amount)] = card;
        return card;
    }

    // 미니언 강화 카드의 다음 레벨 (슬라임 능력 인벤토리가 아니라 MinionEnhanceSystem 기준)
    public int GetMinionEnhanceLevel() => IsMinionEnhance ? MinionEnhanceSystem.GetLevel(minionKey, minionStat) : 0;

    static readonly System.Collections.Generic.Dictionary<RuneData, AbilityData> runeCards = new System.Collections.Generic.Dictionary<RuneData, AbilityData>();
    public static AbilityData CreateRuneCard(RuneData rune)
    {
        if (rune == null)
            return null;
        if (runeCards.TryGetValue(rune, out AbilityData card) && card != null)
            return card;
        card = CreateInstance<AbilityData>();
        card.name = card.key = rune.key;
        card.category = AbilityCategory.Rune;
        card.title = rune.Title;
        card.thum = rune.thum;
        card.maxLevel = int.MaxValue; // 몇 번이든 나옴
        card.runeTile = rune;
        runeCards[rune] = card;
        return card;
    }

    // level = 고른 뒤 레벨 (1부터)
    public string GetDescription(int level)
    {
        if (IsMinionEnhance)
            return MinionEnhanceSystem.GetDescription(minionStat, level, minionAmount);
        if (runeTile != null) // 룬 카드 = 그 룬 Lv.1 설명
            return runeTile.runePrefab is TileRune tileRune ? tileRune.GetDescription(runeTile) : runeTile.desc;
        if (string.IsNullOrEmpty(desc) || prefab == null || !desc.Contains("{0}"))
            return desc;
        return string.Format(desc, prefab.GetDescArgs(level));
    }
}

// 레벨업 카드 종류 (에셋에 번호로 저장 → 뒤에만 추가)
public enum AbilityCategory
{
    Stat,        // 슬라임 능력치 (Lv.2~4 능력 뽑기)
    Race,        // 종족 (Lv.1)
    Job,         // 직업 (Lv.5)
    Passive,     // 패시브 스킬 (Lv.15)
    ActiveSkill, // 발동 스킬 (Lv.20)
    TileExpand,  // 버프 칸 확장 (Lv.10, 2026-11-21)
    Rune,        // 룬 (2026-11-21, 능력치 카드 대신: 타일을 골라 룬을 새김)
    RaceUpgrade, // (2026-11-23) 단계 이름 전용: 종족 강화 (카드는 Race 카드 레벨 +1)
    JobUpgrade,  // (2026-11-23) 단계 이름 전용: 직업 강화 (카드는 Job 카드 레벨 +1)
    Hero,        // (2026-11-23) 용사 (종족+직업 통합, Lv.1)
    HeroUpgrade, // (2026-11-23) 단계 이름 전용: 용사 강화 (카드는 Hero 카드 레벨 +1)
    HeroSkill,   // (2026-11-24) 단계: 스킬 해방 (Lv.2·5·8, 카드는 고른 용사 카드 → 다음 스킬 표시)
    HeroBoost,   // (2026-11-24) 단계: 용사 강화 (Lv.3·4·6·7) = 정예병 버프 강화(용사 카드) 또는 에너지 카드. 에너지 카드 에셋의 종류이기도 함
    MinionEnhance, // (2026-11-25) 미니언 강화 (종 × 스탯, 이전 슬라임 능력치 카드 대체). 레벨업마다 단계 카드 다음에
}
