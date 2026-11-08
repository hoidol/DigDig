using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultCanvas : CanvasUI<ResultCanvas>
{
    public TMP_Text curWaveText;
    public TMP_Text bestWaveText;
    public GameObject newRecordObj;
    public RewardPanel[] rewardPanels;



    public override void OpenCanvas(Action closeCallback = null)
    {

        base.OpenCanvas(closeCallback);

        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        string stageKey = GameManager.Instance.stageData.key;
        // 한 판 = 결과 화면까지 (부활해도 같은 판)
        UserDataManager.Instance.userStageManager.TryStage(stageKey);
        GameEventBus.Publish(new TryStageEvent(stageKey));

        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(stageKey);
        // wave 표시 UI가 씬에 없어도 보상은 지급되도록 (없으면 표시만 생략). 제목(최종/최고 웨이브)은 UI에 있으므로 숫자만
        if (curWaveText != null)
            curWaveText.text = (GameManager.Instance.waveIdx + 1).ToString();
        if (bestWaveText != null)
            bestWaveText.text = (Mathf.Max(userStage.maxWave, GameManager.Instance.waveIdx) + 1).ToString();
        if (newRecordObj != null)
            newRecordObj.SetActive(GameManager.Instance.waveIdx > userStage.maxWave);


        int gold = GameManager.Instance.waveIdx;

        if (gold > 0)
            UserDataManager.Instance.AddGold(gold);

        //최대 깊이에 따라서 보상 받게 하자
        // float distance = Vector2.Distance(Character.Instance.transform.position,Vector2.zero); 
        // distanceText.text = distance.ToString();            

        for (int i = 0; i < rewardPanels.Length; i++)
            rewardPanels[i].gameObject.SetActive(false);

        List<RewardData> rewardDatas = new List<RewardData>();

        int exp = GameManager.Instance.waveIdx / 2;
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
                rewardPanels[i].gameObject.SetActive(true);
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
