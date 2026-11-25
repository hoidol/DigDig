using System.Collections.Generic;
using UnityEngine;

// (2026-11-20) 킹 성장(레벨업 카드) 설정. Resources/KingGrowthSetting
// 카드 풀은 AbilityData 참조 목록 → 넣고 빼는 것만으로 카드 구성을 바꿈
// (2026-11-25) 안 쓰는 능력치·종족·직업·패시브·버프 칸 확장 목록 삭제
[CreateAssetMenu(fileName = "KingGrowthSetting", menuName = "KingSlime/KingGrowthSetting")]
public class KingGrowthSetting : ScriptableObject
{
    public const string RESOURCE_PATH = "KingGrowthSetting";

    [Tooltip("(2026-11-23) 용사 (Lv.1에 랜덤 3장 제시). 넣고 빼면 바로 반영")]
    public AbilityData[] heroAbilities;
    [Tooltip("(2026-11-25) 용사 강화 단계(Lv.3·4·6·7) 카드: 정예 칸 추가·충전 가속·에너지 보유 증가·1·2단계 스킬 강화 (LevelUpChoiceRule.PickHeroBoostChoices)")]
    public AbilityData[] heroBoostAbilities;
    [Tooltip("룬 후보 (엘리트·보스 보상 룬, ItemData key). 타일 고치기(PioneerTile)는 막힌 칸이 있을 때만 나옴")]
    public string[] runeKeys =
    {
        "AttackPowerTile", "AttackSpeedTile", "CritPowerTile", "SkillCooldownTile", "RangeUpTile", "PioneerTile",
        "GiantSlayerTile", "BurstShotTile", "MultiTargetTile", "GrowthTile", "ExpTile", "SlowTile", "StunTile",
    };

    static KingGrowthSetting instance;
    public static KingGrowthSetting Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<KingGrowthSetting>(RESOURCE_PATH);
            return instance;
        }
    }
}
