using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultCanvas : CanvasUI<ResultCanvas>
{
    public GameObject clearObj;
    public GameObject failObj;
    public TMP_Text stageText;
    public TMP_Text distanceText;
    // public TMP_Text memorPieceCountText;
    public RewardPanel[] rewardPanels;

    bool clear;

    public void OpenCanvas(bool clear, Action closeCallback = null)
    {
        this.clear = clear;
        base.OpenCanvas(closeCallback);
        clearObj.SetActive(false);
        failObj.SetActive(false);
        if (clear)
        {
            clearObj.SetActive(true);
            
        }
        else
        {
            failObj.SetActive(true);;   
        }
        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(GameManager.Instance.stageData.key);        
        UserStage curUserStage = UserDataManager.Instance.userStageManager.GetCurrentStage();

        stageText.text = GameManager.Instance.stageData.order.ToString();

        
        int exp =0;
        if(userStage.clearCount <= 0)
        {
            exp = GameManager.Instance.phase * 5;
        }else
        {
            if (clear)
            {
                exp = 10;
            }
        }

        if(exp>0)
            UserDataManager.Instance.userMyInfoManager.AddExp(exp); 
        
        int destroy = GameManager.Instance.destroyStoneCount + GameManager.Instance.killEnemyCount;
        int gold = destroy / 10;  
        
        if(userStage.clearCount > 0)
        {
            int orderGap = (StageManager.Instance.GetStageData(curUserStage.key).order - GameManager.Instance.stageData.order)+1;
            gold = (int)(gold/orderGap);
        }

        if(gold>0)
            UserDataManager.Instance.AddGold(gold);  

        //최대 깊이에 따라서 보상 받게 하자
        float distance = Vector2.Distance(Character.Instance.transform.position,Vector2.zero); 
        distanceText.text = distance.ToString();

        int drawTicket = Mathf.FloorToInt(distance);
        if(userStage.clearCount > 0)
        {
            int orderGap = (StageManager.Instance.GetStageData(curUserStage.key).order - GameManager.Instance.stageData.order)+1;
            drawTicket = (int)(distance/orderGap);
        }
        
        if(drawTicket>0)
            UserDataManager.Instance.AddDrawTicket(drawTicket);  

        for(int i = 0; i < rewardPanels.Length; i++)
            rewardPanels[i].gameObject.SetActive(false);
        
        List<RewardData> rewardDatas = new List<RewardData>();
        if(exp > 0)
        {
            rewardDatas.Add(new RewardData()
            {
                rewardType = RewardType.Exp,
                value = exp.ToString()
            });
        }
        if(drawTicket > 0)
        {
            rewardDatas.Add(new RewardData()
            {
                rewardType = RewardType.DrawTicket,
                value = drawTicket.ToString()
            });
        }
        
        if(gold > 0)
        {
            rewardDatas.Add(new RewardData()
            {
                rewardType = RewardType.Gold,
                value = gold.ToString()
            });
        }

        for(int i = 0; i < rewardDatas.Count; i++)
        {
            if (i < rewardDatas.Count)
            {
                rewardPanels[i].SetRewardData(rewardDatas[i],null);
            }
        }        

        UserDataManager.Instance.userStageManager.EndStage(GameManager.Instance.stageData.key, clear, clear? GameManager.Instance.phase-1 : GameSetting.BOSS_PHASE-1);
    }

    public void OnClickedHome()
    {
        CloseCanvas();
        FadeCanvs.Instance.FadeOutIn(null, () => { SceneManager.LoadScene("Lobby"); });
    }
}
