
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class BattleCanvas : BaseLobbyCanvas
{
    public GameObject nextStageButton;
    public GameObject preStageButton;
    public StageRewardContainer stageRewardContainer;

    [SerializeField] UserStage curUserStage;
    public StageData stageData;
    public int stageOrder;
    public TMP_Text titleText;
    public Image stageThumImage;
    public GameObject stageLockedGO;
    void Start()
    {
       GameEventBus.Subscribe<ChangedDifficultyEvent>(OnChangedDifficultyEvent);  
    }

    void OnChangedDifficultyEvent(ChangedDifficultyEvent e)
    {
        StageData changedStageData = StageManager.Instance.GetStageData(UserDataManager.difficultyType, stageData.order );
        
        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(changedStageData.key);
        SetUserStage(userStage);
        UpdateCanvas();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        if (curUserStage == null || string.IsNullOrEmpty(curUserStage.key))
        {
            curUserStage = UserDataManager.Instance.userStageManager.GetCurrentStage();
        }
        SetUserStage(curUserStage);
        UpdateCanvas();
    }

    public void SetUserStage(UserStage userStage)
    {
        curUserStage = userStage;
        stageData = StageManager.Instance.GetStageData(curUserStage.key);
        stageRewardContainer.SetStageData(curUserStage, stageData);
        UpdateCanvas();

    }

    public override void UpdateCanvas()
    {
        stageRewardContainer.UpdateContainer();
        titleText.text = stageData.Title;
        stageOrder = stageData.order;

        StageData nextStageData = StageManager.Instance.GetStageData(UserDataManager.difficultyType, stageData.order + 1);
        StageData preStageData = StageManager.Instance.GetStageData(UserDataManager.difficultyType, stageData.order - 1);

        nextStageButton.SetActive(false);
        preStageButton.SetActive(false);

        if (nextStageData != null )//&& curUserStage.clearCount > 0
        {
            nextStageButton.SetActive(true);
        }

        if (preStageData != null)
        {
            preStageButton.SetActive(true);
        }

        stageLockedGO.SetActive(!stageData.ChekcUnlock());
    }


    public void OnClickedNext()
    {
        Debug.Log("OnClickedNext()");
        string stageKey = StageManager.Instance.GetStageData(UserDataManager.difficultyType, stageOrder + 1).key;
        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(stageKey);
        SetUserStage(userStage);
    }
    public void OnClickedPre()
    {
        Debug.Log("OnClickedPre()");
        string stageKey = StageManager.Instance.GetStageData(UserDataManager.difficultyType, stageOrder - 1).key;
        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(stageKey);
        SetUserStage(userStage);
    }
    
    public void OnClickedBtn()
    {
        #if UNITY_EDITOR
        if(UserDataManager.Instance.userData.energe <GameSetting.PLAY_COST_ENERGE)
        {
            ToastCanvas.Toast( TranslateManager.GetText("Not enough energe"));
            return;
        }
        #endif
        UserDataManager.STAGE_KEY = curUserStage.key;
        SceneManager.LoadScene("InGame");
        UserDataManager.Instance.AddEnerge(-GameSetting.PLAY_COST_ENERGE);
    }
}
