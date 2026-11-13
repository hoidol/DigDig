
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class BattleCanvas : BaseLobbyCanvas
{
    // public GameObject nextStageButton;
    // public GameObject preStageButton;
    // public StageRewardContainer stageRewardContainer;

    [SerializeField] UserStage curUserStage;
    public StageData stageData;
    public TMP_Text titleText;
    public Image stageThumImage;
    public GameObject stageLockedGO;
    void Start()
    {
        GameEventBus.Subscribe<ChangedDifficultyEvent>(OnChangedDifficultyEvent);
    }

    void OnChangedDifficultyEvent(ChangedDifficultyEvent e)
    {
        // 난이도마다 스테이지 하나 → 난이도가 바뀌면 그 난이도의 스테이지로
        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(UserDataManager.difficultyType);
        SetUserStage(userStage);
        UpdateCanvas();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        SetUserStage(UserDataManager.Instance.userStageManager.GetCurrentStage());
        UpdateCanvas();
    }

    public void SetUserStage(UserStage userStage)
    {
        curUserStage = userStage;
        stageData = StageManager.Instance.GetStageData(Enum.Parse<DifficultyType>(curUserStage.key));
        // stageRewardContainer.SetStageData(curUserStage, stageData);
        UpdateCanvas();

    }

    public override void UpdateCanvas()
    {
        // stageRewardContainer.UpdateContainer();
        titleText.text = DifficultyDisplayButton.GetDifficultyText(stageData.difficulty); // key(Normal 등) 대신 번역된 난이도 이름

        // 난이도마다 스테이지 하나라 이전/다음 스테이지 이동 없음
        // nextStageButton.SetActive(false);
        // preStageButton.SetActive(false);

        stageLockedGO.SetActive(!stageData.ChekcUnlock());
    }


    public void OnClickedBtn()
    {
        if (!stageData.ChekcUnlock())
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Difficulty_Locked"), StageData.UNLOCK_WAVE));
            return;
        }
        // 게임 시작 = 에너지 PLAY_COST_ENERGE 소모. 모자라면 시작 안 함
        if (!EnergeManager.TryUsePlayCost())
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Energe_NotEnough"), EnergeManager.CHARGE_INTERVAL_MINUTES));
            return;
        }
        SceneManager.LoadScene("InGame");
    }
}
