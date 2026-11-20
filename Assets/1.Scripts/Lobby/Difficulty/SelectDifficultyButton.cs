using TMPro;
using UnityEngine;

public class SelectDifficultyButton : ButtonUI
{
    public DifficultyType difficultyType;

    // 비워 두면 자식 TMP_Text 순서대로(0 = 이름, 1 = 설명) 자동 연결
    public TMP_Text titleText;
    public TMP_Text descText;
    public GameObject highlightObject;
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

    // 지금 선택한 스테이지(UserDataManager.stageOrder)에 이 난이도가 있는지 (없으면 SelectDifficultyCanvas가 버튼을 숨김)
    public bool HasStageData => StageManager.Instance != null && StageManager.Instance.GetStageData(UserDataManager.stageOrder, difficultyType) != null;

    public void UpdateButton()
    {
        if (!HasStageData)
            return;

        if (titleText != null)
            titleText.text = DifficultyDisplayButton.GetDifficultyText(difficultyType);
        if (descText != null)
            descText.text = GetDescText(difficultyType);

        highlightObject.SetActive(UserDataManager.difficultyType == difficultyType); 
    }

    // 잠김 = 해금 조건, 열림 = 최고 기록 wave (기록 없으면 "기록 없음")
    static string GetDescText(DifficultyType difficultyType)
    {
        StageData stageData = StageManager.Instance.GetStageData(UserDataManager.stageOrder, difficultyType);
        if (stageData != null && !stageData.ChekcUnlock())
            return stageData.GetLockedMessage();

        UserStage userStage = UserDataManager.Instance.userStageManager.FindUserStage(UserDataManager.stageOrder, difficultyType);
        if (userStage == null || userStage.tryCount <= 0)
            return TranslateManager.GetText("Difficulty_NoRecord");
        return string.Format(TranslateManager.GetText("Difficulty_BestWave"), userStage.maxWave + 1);
    }

    public override void OnClickedBtn()
    {
        SelectDifficultyCanvas.Instance.SelectedDifficulty(difficultyType);
    }
}
