using System.Collections.Generic;
using UnityEngine;

// (2026-11-20) 킹 성장(레벨업 카드) 설정. Resources/KingGrowthSetting
// 카드 풀은 AbilityData 참조 목록 → 넣고 빼는 것만으로 카드 구성을 바꿈
[CreateAssetMenu(fileName = "KingGrowthSetting", menuName = "KingSlime/KingGrowthSetting")]
public class KingGrowthSetting : ScriptableObject
{
    public const string RESOURCE_PATH = "KingGrowthSetting";

    [Tooltip("능력 뽑기 Panel에 나오는 킹 능력치 카드")]
    public AbilityData[] statAbilities;
    [Tooltip("종족 (첫 레벨업에서 랜덤 3개 제시)")]
    public AbilityData[] raceAbilities;
    [Tooltip("직업 (Lv.5 레벨업에서 랜덤 3개 제시)")]
    public AbilityData[] jobAbilities;
    [Tooltip("버프 칸 확장 (Lv.10, 같은 카드 3장에 서로 다른 추가 칸)")]
    public AbilityData tileExpandAbility;
    [Tooltip("레벨업 룬 카드 후보 (ItemData key). 타일 고치기(PioneerTile)는 막힌 칸이 있을 때만 나옴")]
    public string[] runeKeys =
    {
        "AttackPowerTile", "AttackSpeedTile", "CritPowerTile", "SkillCooldownTile", "RangeUpTile", "PioneerTile",
        "GiantSlayerTile", "BurstShotTile", "MultiTargetTile", "GrowthTile", "ExpTile", "SlowTile", "StunTile",
    };
    [Tooltip("패시브 (Lv.15, 종족별: prefab의 PassiveKingAbility.raceType이 킹 종족과 같은 것만 제시)")]
    public AbilityData[] passiveAbilities;

    // 그 종족 패시브만
    public List<AbilityData> GetRacePassives(KingRaceType race)
    {
        var list = new List<AbilityData>();
        if (passiveAbilities == null)
            return list;
        foreach (var d in passiveAbilities)
            if (d != null && d.prefab is PassiveKingAbility p && p.raceType == race)
                list.Add(d);
        return list;
    }

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
