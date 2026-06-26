
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyCanvas : MonoBehaviour 
{
    //public StartStageButton startStageButton;
    public GameObject nextStageButton;
    public GameObject preStageButton;

    UserStage curUserStage;
    public int stageOrder;
    public TMP_Text titleText;
    void Start()
    {
        UserStage curUserStage= UserManager.Instance.userStageManager.GetCurrentStage();
        stageOrder = StageData.GetStageData(curUserStage.key).order;
        SetUserStage(curUserStage);
    }
    public void UpdateCanvas()
    {
       
    }

    public void SetUserStage(UserStage userStage)
    {
        curUserStage = userStage;
        StageData stageData = StageManager.Instance.GetStageData(curUserStage.key);
        titleText.text = stageData.Title;

        StageData nextStageData = StageManager.Instance.GetStageData(stageData.order +1);
        StageData preStageData = StageManager.Instance.GetStageData(stageData.order -1);

        nextStageButton.SetActive(false);
        preStageButton.SetActive(false);

        if(nextStageData != null && userStage.clearCount > 0)
        {
            nextStageButton.SetActive(true);
        }

        if(preStageData != null)
        {
            preStageButton.SetActive(true);
        }
    }

    public void OnClickedNext()
    {
        string stageKey = StageManager.Instance.GetStageData(stageOrder+1).key;
        UserStage userStage = UserManager.Instance.userStageManager.GetUserStage(stageKey); 
        SetUserStage(userStage);
    }
    public void OnClickedPre()
    {
        string stageKey = StageManager.Instance.GetStageData(stageOrder-1).key;
        UserStage userStage = UserManager.Instance.userStageManager.GetUserStage(stageKey); 
        SetUserStage(userStage);
    }

    public void OnClickedBtn()
    {
        UserManager.STAGE_KEY = curUserStage.key;        
        SceneManager.LoadScene("InGame");
    }
}