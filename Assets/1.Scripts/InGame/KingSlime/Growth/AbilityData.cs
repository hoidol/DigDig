using UnityEngine;

// (2026-11-20) 킹 성장 능력 데이터 (레벨업 카드 한 장). prefab의 KingAbility 컴포넌트가 실제 동작
// 능력을 바꾸려면 prefab 참조만 바꾸면 됨. 이름·설명은 아직 번역 전이라 에셋에 직접 입력(임시)
[CreateAssetMenu(fileName = "AbilityData", menuName = "KingSlime/AbilityData")]
public class AbilityData : ScriptableObject
{
    public string key;
    public AbilityCategory category;
    public string title;
    [TextArea]
    [Tooltip("{0}, {1} … = 고르면 될 레벨의 수치 (KingAbility.GetDescArgs)")]
    public string desc;
    public Sprite thum;
    [Min(1)] public int maxLevel = 5;
    public KingAbility prefab;
    // (2026-11-21) 룬 카드: 고르면 타일을 골라 그 룬을 새김(같은 룬이면 레벨 +1). 실행 중 ItemData로 만듦(CreateRuneCard), 에셋 없음
    [System.NonSerialized] public ItemData runeItem;
    public bool IsRune => runeItem != null;

    static readonly System.Collections.Generic.Dictionary<ItemData, AbilityData> runeCards = new System.Collections.Generic.Dictionary<ItemData, AbilityData>();
    public static AbilityData CreateRuneCard(ItemData item)
    {
        if (item == null)
            return null;
        if (runeCards.TryGetValue(item, out AbilityData card) && card != null)
            return card;
        card = CreateInstance<AbilityData>();
        card.name = card.key = item.key;
        card.category = AbilityCategory.Rune;
        card.title = item.Title;
        card.thum = item.thum;
        card.maxLevel = int.MaxValue; // 몇 번이든 나옴
        card.runeItem = item;
        runeCards[item] = card;
        return card;
    }

    // level = 고른 뒤 레벨 (1부터)
    public string GetDescription(int level)
    {
        if (runeItem != null) // 룬 카드 = 그 룬 Lv.1 설명
            return runeItem.itemPrefab is TileItem tileItem ? tileItem.GetDescription(runeItem) : runeItem.desc;
        if (string.IsNullOrEmpty(desc) || prefab == null || !desc.Contains("{0}"))
            return desc;
        return string.Format(desc, prefab.GetDescArgs(level));
    }
}

// 레벨업 카드 종류 (에셋에 번호로 저장 → 뒤에만 추가)
public enum AbilityCategory
{
    Stat,        // 킹 능력치 (Lv.2~4 능력 뽑기)
    Race,        // 종족 (Lv.1)
    Job,         // 직업 (Lv.5)
    Passive,     // 패시브 스킬 (Lv.15)
    ActiveSkill, // 발동 스킬 (Lv.20)
    TileExpand,  // 버프 칸 확장 (Lv.10, 2026-11-21)
    Rune,        // 룬 (2026-11-21, 능력치 카드 대신: 타일을 골라 룬을 새김)
}
