using UnityEngine;

// 기능 해금 조건 하나 (ConditionData와 같은 모양: 종류 + value + count).
// ConditionData는 인게임 능력·탄 CSV가 enum 이름으로 읽고 있어서 건드리지 않고 따로 둔다.
// 조건 종류 추가 = UnlockConditionType에 값 추가(뒤에만) + GetCurrent / GetLockedMessage에 한 줄
[System.Serializable]
public class UnlockCondition
{
    public UnlockConditionType conditionType;
    [Tooltip("BestWave: 난이도 이름(Normal/Hard/Hell). 비우면 모든 난이도 중 최고 기록")]
    public string value;
    [Tooltip("필요한 값. 유저 레벨 또는 wave 번호(화면에 보이는 번호, 1부터)")]
    public int count;

    public bool IsMet()
    {
        return IsMet(GetCurrent(), count);
    }

    public static bool IsMet(int current, int need)
    {
        return current >= need;
    }

    // 지금 값 (조건 종류별). 판단할 수 없는 상황(로비에서 CurrentWave 등)은 0 → 잠김
    public int GetCurrent()
    {
        switch (conditionType)
        {
            case UnlockConditionType.UserLevel:
                return UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv;

            case UnlockConditionType.BestWave:
                return GetBestWave(value);

            case UnlockConditionType.CurrentWave:
                GameManager gameManager = GameManager.Instance;
                return gameManager != null ? gameManager.waveIdx + 1 : 0;
        }
        return 0;
    }

    // 최고 도달 wave (표시 번호 = 저장값 maxWave + 1). 난이도를 비우면 모든 난이도 중 가장 높은 기록
    static int GetBestWave(string difficulty)
    {
        UserStageManager userStageManager = UserDataManager.Instance.userStageManager;
        if (!string.IsNullOrEmpty(difficulty))
            return userStageManager.GetUserStage(difficulty).maxWave + 1;

        int best = 0;
        foreach (UserStage userStage in userStageManager.userStageData.userStages)
            best = Mathf.Max(best, userStage.maxWave + 1);
        return best;
    }

    // 잠겨 있을 때 보여줄 안내 문구
    public string GetLockedMessage()
    {
        switch (conditionType)
        {
            case UnlockConditionType.UserLevel:
                return string.Format(TranslateManager.GetText("Unlock_UserLevel"), count);

            case UnlockConditionType.BestWave:
                if (string.IsNullOrEmpty(value))
                    return string.Format(TranslateManager.GetText("Unlock_BestWave"), count);
                return string.Format(TranslateManager.GetText("Unlock_BestWaveDifficulty"),
                    TranslateManager.GetText($"Difficulty.{value}"), count);

            case UnlockConditionType.CurrentWave:
                return string.Format(TranslateManager.GetText("Unlock_CurrentWave"), count);
        }
        return "";
    }
}

// 인스펙터에 번호로 저장되므로 뒤에만 추가
public enum UnlockConditionType
{
    UserLevel,   // 유저 레벨 count 이상
    BestWave,    // 최고 도달 wave count 이상 (value = 난이도, 비우면 전체 중 최고)
    CurrentWave, // 인게임: 지금 진행 중인 wave count 이상
}
