using System.Collections.Generic;
using UnityEngine;

// (2026-11-23) 한 판의 미션 목록. Resources/InGameMissionSetting 에셋 인스펙터에서 추가·수치 수정 (종류 = Mission 하위 클래스)
// 에셋이 없거나 비어 있으면 코드 기본 목록(CreateDefaults)
[CreateAssetMenu(fileName = "InGameMissionSetting", menuName = "KingSlime/InGameMissionSetting")]
public class InGameMissionSetting : ScriptableObject
{
    public const string RESOURCE_PATH = "InGameMissionSetting";

    [SerializeReference]
    public List<Mission> missions = CreateDefaults();

    public static List<Mission> CreateDefaults() => new List<Mission>
    {
        new ClearWaveWithMinionsMission { waveNumber = 1, maxMinionCount = 1 },
        new PlaceMinionsMission { minLevel = 1, count = 5, distinctKeys = true },
        new PlaceMinionsMission { minLevel = 2, count = 5, distinctKeys = true },
        new PlaceMinionsMission { minLevel = 3, count = 1, distinctKeys = false, exactLevel = false },
        new ClearWaveWithMinionsMission { waveNumber = 5, maxMinionCount = 4 },
    };

    static InGameMissionSetting instance;
    public static InGameMissionSetting Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<InGameMissionSetting>(RESOURCE_PATH);
            return instance;
        }
    }

    // 이번 판 미션 (null 칸은 건너뜀)
    public static List<Mission> GetMissions()
    {
        var result = new List<Mission>();
        var s = Instance;
        if (s != null && s.missions != null)
        {
            foreach (var m in s.missions)
            {
                if (m != null)
                    result.Add(m);
            }
        }
        return result.Count > 0 ? result : CreateDefaults();
    }
}
