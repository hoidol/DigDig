
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyCanvas : CanvasUI<LobbyCanvas>
{
    public GameObject nextStageButton;
    public GameObject preStageButton;

    UserStage curUserStage;
    public int stageOrder;
    public TMP_Text titleText;

    public void UpdateCanvas()
    {
        if (curUserStage == null)
        {
            curUserStage = UserManager.Instance.userStageManager.GetCurrentStage();
        }
        StageData stageData = StageManager.Instance.GetStageData(curUserStage.key);
        titleText.text = stageData.Title;
        stageOrder = stageData.order;

        StageData nextStageData = StageManager.Instance.GetStageData(stageData.order + 1);
        StageData preStageData = StageManager.Instance.GetStageData(stageData.order - 1);

        nextStageButton.SetActive(false);
        preStageButton.SetActive(false);

        if (nextStageData != null && curUserStage.clearCount > 0)
        {
            nextStageButton.SetActive(true);
        }

        if (preStageData != null)
        {
            preStageButton.SetActive(true);
        }
    }

    public void SetUserStage(UserStage userStage)
    {
        curUserStage = userStage;
        UpdateCanvas();

    }

    public void OnClickedNext()
    {
        string stageKey = StageManager.Instance.GetStageData(stageOrder + 1).key;
        UserStage userStage = UserManager.Instance.userStageManager.GetUserStage(stageKey);
        SetUserStage(userStage);
    }
    public void OnClickedPre()
    {
        string stageKey = StageManager.Instance.GetStageData(stageOrder - 1).key;
        UserStage userStage = UserManager.Instance.userStageManager.GetUserStage(stageKey);
        SetUserStage(userStage);
    }

    public void OnClickedBtn()
    {
        UserManager.STAGE_KEY = curUserStage.key;
        SceneManager.LoadScene("InGame");
    }
}