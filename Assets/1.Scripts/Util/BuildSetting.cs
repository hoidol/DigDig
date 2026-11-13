using UnityEngine;

[CreateAssetMenu(fileName = "BuildSetting", menuName = "Settings/BuildSetting", order = 0)]
public class BuildSetting : ScriptableObject
{
    private static BuildSetting instance;

    // 어디서든 쉽게 접근 가능한 인스턴스
    public static BuildSetting Instance
    {
        get
        {
            if (instance == null)
            {
                // Resources 폴더에서 자동으로 BuildSetting.asset을 로드
                instance = Resources.Load<BuildSetting>("BuildSetting");
                if (instance == null)
                {
                    Debug.LogError("BuildSetting.asset이 Resources 폴더에 없습니다!");
                }
            }
            return instance;
        }
    }

    [Header("Build Settings")]
    public bool isTestMode = false;

    [Header("체크하면 에너지를 항상 최대로 취급 (게임 시작 시 차감 안 함, 저장값은 그대로)")]
    public bool ignoreEnerge = false;

    [Header("체크하면 튜토리얼을 시작하지 않음 (완료 기록은 그대로)")]
    public bool skipTutorial = false;
}
