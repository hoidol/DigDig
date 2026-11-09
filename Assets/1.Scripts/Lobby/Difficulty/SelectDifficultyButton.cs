using TMPro;
using UnityEngine;

public class SelectDifficultyButton : ButtonUI
{
    public DifficultyType difficultyType;

    // 비워 두면 자식 TMP_Text 순서대로(0 = 이름, 1 = 설명) 자동 연결
    public TMP_Text titleText;
    public TMP_Text descText;

    public override void Awake()
    {
        base.Awake();
        if (titleText == null || descText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            if (titleText == null && texts.Length > 0)
                titleText = texts[0];
            if (descText == null && texts.Length > 1)
                descText = texts[1];
        }
    }

    void OnEnable()
    {
        UpdateButton();
    }

    public void UpdateButton()
    {
        if (titleText != null)
            titleText.text = DifficultyDisplayButton.GetDifficultyText(difficultyType);
        if (descText != null)
            descText.text = GetDescText(difficultyType);
    }

    // 잠김 = 해금 조건, 열림 = 최고 기록 wave (기록 없으면 "기록 없음")
    static string GetDescText(DifficultyType difficultyType)
    {
        StageData stageData = StageManager.Instance.GetStageData(difficultyType);
        if (stageData != null && !stageData.ChekcUnlock())
            return string.Format(TranslateManager.GetText("Difficulty_Locked"), StageData.UNLOCK_WAVE);

        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(difficultyType);
        if (userStage == null || userStage.tryCount <= 0)
            return TranslateManager.GetText("Difficulty_NoRecord");
        return string.Format(TranslateManager.GetText("Difficulty_BestWave"), userStage.maxWave + 1);
    }

    public override void OnClickedBtn()
    {
        SelectDifficultyCanvas.Instance.SelectedDifficulty(difficultyType);
    }
}
