using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultCanvas : CanvasUI<ResultCanvas>
{
    public GameObject clearObj;
    public GameObject failObj;
    public TMP_Text curWaveText;
    public TMP_Text bestWaveText;
    public GameObject newRecordObj;
    // public TMP_Text distanceText;
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
            failObj.SetActive(true); ;
        }
        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(GameManager.Instance.stageData.key);
        curWaveText.text = $"Current Wave: {GameManager.Instance.waveIdx+1}";
        bestWaveText.text = $"Best Wave: {userStage.maxWave+1}";

        if(GameManager.Instance.waveIdx > userStage.maxWave)
        {
            newRecordObj.SetActive(true);

        }
        else
        {
            newRecordObj.SetActive(false);
        }
        

        int gold = GameManager.Instance.waveIdx;

        if (gold > 0)
            UserDataManager.Instance.AddGold(gold);

        //최대 깊이에 따라서 보상 받게 하자
        // float distance = Vector2.Distance(Character.Instance.transform.position,Vector2.zero); 
        // distanceText.text = distance.ToString();            

        for (int i = 0; i < rewardPanels.Length; i++)
            rewardPanels[i].gameObject.SetActive(false);

        List<RewardData> rewardDatas = new List<RewardData>();

        int exp = GameManager.Instance.waveIdx/2;
        UserDataManager.Instance.userMyInfoManager.AddExp(exp);
        if (exp > 0)
        {
            rewardDatas.Add(new RewardData()
            {
                rewardType = RewardType.Exp,
                value = exp.ToString()
            });
        }

        int drawTicket = SlimeSpawner.Instance.mergeCount;
        if (drawTicket > 0)
        {
            UserDataManager.Instance.AddDrawTicket(drawTicket);  

            rewardDatas.Add(new RewardData()
            {
                rewardType = RewardType.DrawTicket,
                value = drawTicket.ToString()
            });
        }

        if (gold > 0)
        {
            rewardDatas.Add(new RewardData()
            {
                rewardType = RewardType.Gold,
                value = gold.ToString()
            });
        }

        for (int i = 0; i < rewardPanels.Length; i++)
        {
            if (i < rewardDatas.Count)
            {
                rewardPanels[i].SetRewardData(rewardDatas[i], null);
            }
            else
            {
                rewardPanels[i].gameObject.SetActive(false);
            }
        }

        UserDataManager.Instance.userStageManager.EndStage(GameManager.Instance.stageData.key, GameManager.Instance.waveIdx); // 무한 웨이브: 마지막으로 넘긴 wave
    }

    public void OnClickedHome()
    {
        CloseCanvas();
        FadeCanvs.Instance.FadeOutIn(null, () => { SceneManager.LoadScene("Lobby"); });
    }
}
