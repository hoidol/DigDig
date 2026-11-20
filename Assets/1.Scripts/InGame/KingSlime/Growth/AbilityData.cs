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
    [Tooltip("{0} = 고르면 될 레벨의 수치 (KingAbility.GetValueText)")]
    public string desc;
    public Sprite thum;
    [Min(1)] public int maxLevel = 5;
    public KingAbility prefab;

    // level = 고른 뒤 레벨 (1부터)
    public string GetDescription(int level)
    {
        if (string.IsNullOrEmpty(desc) || prefab == null || !desc.Contains("{0}"))
            return desc;
        return string.Format(desc, prefab.GetValueText(level));
    }
}

// 레벨업 카드 종류 (에셋에 번호로 저장 → 뒤에만 추가)
public enum AbilityCategory
{
    Stat,        // 킹 능력치 (Lv.2~4 능력 뽑기)
    Race,        // 종족 (Lv.1)
    Job,         // 직업 (Lv.5)
    Passive,     // 패시브 스킬 (Lv.10)
    ActiveSkill, // 발동 스킬 (Lv.15)
}
