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
