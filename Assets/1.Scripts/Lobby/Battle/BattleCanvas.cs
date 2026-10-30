
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class BattleCanvas : BaseLobbyCanvas
{
    // public GameObject nextStageButton;
    // public GameObject preStageButton;
    public StageRewardContainer stageRewardContainer;

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
        stageData = StageManager.Instance.GetStageData(Enum.Parse<DifficultyType>(curUserStage.key) );
        // stageRewardContainer.SetStageData(curUserStage, stageData);
        UpdateCanvas();

    }

    public override void UpdateCanvas()
    {
        stageRewardContainer.UpdateContainer();
        titleText.text = stageData.Title;

        // 난이도마다 스테이지 하나라 이전/다음 스테이지 이동 없음
        // nextStageButton.SetActive(false);
        // preStageButton.SetActive(false);

        stageLockedGO.SetActive(!stageData.ChekcUnlock());
    }


    public void OnClickedBtn()
    {
        // #if !UNITY_EDITOR
        // if(UserDataManager.Instance.userData.energe <GameSetting.PLAY_COST_ENERGE)
        // {
        //     ToastCanvas.Toast( TranslateManager.GetText("Not enough energe"));
        //     return;
        // }
        // #endif
        // UserDataManager.STAGE_KEY = curUserStage.key;
        SceneManager.LoadScene("InGame");
        UserDataManager.Instance.AddEnerge(-GameSetting.PLAY_COST_ENERGE);
    }
}
