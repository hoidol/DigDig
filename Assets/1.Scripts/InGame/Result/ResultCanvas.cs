using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultCanvas : CanvasUI<ResultCanvas>
{
    public TMP_Text curWaveText;
    public TMP_Text bestWaveText;
    public GameObject newRecordObj;
    public RewardPanel[] rewardPanels;
    public GameObject adDoubleRewardButton; // 광고 보고 보상 2배 (한 판에 1번)

    // 이번 결과 화면에서 지급한 기본 보상 (2배 = 같은 양을 한 번 더 지급)
    int rewardGold;
    int rewardExp;
    int rewardDrawTicket;
    bool isDoubled;
    bool isWatchingAd; // 광고 보는 동안 중복 클릭·홈 이동 방지

    public bool IsDoubled => isDoubled;

    public override void OpenCanvas(Action closeCallback = null)
    {

        base.OpenCanvas(closeCallback);

        isDoubled = false;
        isWatchingAd = false;
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

        // 최대 레벨이면 경험치 보상 없음 (0으로 유지)
        var myInfo = UserDataManager.Instance.userMyInfoManager;
        rewardExp = UserMyInfoManager.GetGainExp(myInfo.userMyInfoData.lv, GameManager.Instance.waveIdx / 2);
        myInfo.AddExp(rewardExp);
        UserMyInfoManager.unshownGainExp += rewardExp; // 로비에서 바가 차오르는 연출

        rewardDrawTicket = SlimeFieldController.Instance.mergeCount;
        if (rewardDrawTicket > 0)
            UserDataManager.Instance.AddDrawTicket(rewardDrawTicket);

        rewardGold = gold;

        ShowRewards();

        UserDataManager.Instance.userStageManager.EndStage(GameManager.Instance.stageData.key, GameManager.Instance.waveIdx); // 무한 웨이브: 마지막으로 넘긴 wave
    }

    // 보상 패널 표시 (2배 받았으면 2배 값으로). animate = 값이 바뀐 걸 보여주는 글씨 확대 이펙트
    void ShowRewards(bool animate = false)
    {
        int mul = isDoubled ? 2 : 1;
        List<RewardData> rewardDatas = new List<RewardData>();
        if (rewardExp > 0)
            rewardDatas.Add(new RewardData() { rewardType = RewardType.Exp, value = (rewardExp * mul).ToString() });
        if (rewardDrawTicket > 0)
            rewardDatas.Add(new RewardData() { rewardType = RewardType.DrawTicket, value = (rewardDrawTicket * mul).ToString() });
        if (rewardGold > 0)
            rewardDatas.Add(new RewardData() { rewardType = RewardType.Gold, value = (rewardGold * mul).ToString() });

        for (int i = 0; i < rewardPanels.Length; i++)
        {
            if (i < rewardDatas.Count)
            {
                rewardPanels[i].gameObject.SetActive(true);
                if (animate)
                    rewardPanels[i].PlayValueChange(rewardDatas[i]);
                else
                    rewardPanels[i].SetRewardData(rewardDatas[i], null);
            }
            else
            {
                rewardPanels[i].gameObject.SetActive(false);
            }
        }

        // 받을 보상이 없거나 이미 2배를 받았으면 버튼 숨김
        if (adDoubleRewardButton != null)
            adDoubleRewardButton.SetActive(!isDoubled && rewardDatas.Count > 0);
    }

    // 광고 보고 보상 2배 (경험치·뽑기권·골드 모두). 광고 성공 시에만, 광고 제거 구매자는 광고 없이
    public void OnClickedDoubleReward()
    {
        if (isDoubled || isWatchingAd)
            return;
        if (rewardExp <= 0 && rewardDrawTicket <= 0 && rewardGold <= 0)
            return;

        if (UserDataManager.Instance.isAdRemoved)
        {
            GiveDoubleReward();
            return;
        }

        isWatchingAd = true;
        AdManager.Show("result_double_reward", AdType.RA, async (success) =>
        {
            await UniTask.SwitchToMainThread(); // 광고 SDK 콜백은 메인 스레드가 아닐 수 있음
            isWatchingAd = false;
            if (success)
                GiveDoubleReward();
        });
    }

    // 기본 보상과 같은 양을 한 번 더 지급 → 합계 2배
    void GiveDoubleReward()
    {
        if (isDoubled)
            return;
        isDoubled = true;

        if (rewardExp > 0)
        {
            UserDataManager.Instance.userMyInfoManager.AddExp(rewardExp);
            UserMyInfoManager.unshownGainExp += rewardExp;
        }
        if (rewardDrawTicket > 0)
            UserDataManager.Instance.AddDrawTicket(rewardDrawTicket);
        if (rewardGold > 0)
            UserDataManager.Instance.AddGold(rewardGold);

        ShowRewards(true);
    }

    public void OnClickedHome()
    {
        if (isWatchingAd)
            return;
        CloseCanvas();
        FadeCanvs.Instance.FadeOutIn(null, () => { SceneManager.LoadScene("Lobby"); });
    }
}
