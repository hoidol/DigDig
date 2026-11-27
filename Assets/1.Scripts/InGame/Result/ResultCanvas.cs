using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultCanvas : CanvasUI<ResultCanvas>
{
    protected override void PlayOpenSound() { } // 보상 소리(RewardGet)가 따로 남
    public TMP_Text resultTitleText; // 클리어 = "STAGE CLEAR!" / 아니면 "GAME OVER" (비우면 하위 ResultTitleText)
    public TMP_Text curWaveText;     // "WAVE N"
    // public TMP_Text bestWaveText;
    public GameObject newRecordObj;
    public RewardPanel[] rewardPanels;
    public GameObject adDoubleRewardButton; // 광고 보고 보상 2배 (한 판에 1번)
    // (2026-11-27) 게임 기록: 통계 칸 + 피해 순위 (씬에 만들어 연결: Tools/InGame/Add Result Stats To ResultCanvas)
    public ResultStatsPanel statsPanel;

    // 이번 결과 화면에서 지급한 기본 보상 (2배 = 같은 양을 한 번 더 지급). (2026-11-27) 골드 삭제 → 경험치만
    int rewardExp;
    bool isDoubled;
    bool isWatchingAd; // 광고 보는 동안 중복 클릭·홈 이동 방지

    public bool IsDoubled => isDoubled;

    // (2026-11-26) 클리어 팁 입력 (씬에 만들어 연결: Tools/InGame/Add ClearTip Input To ResultCanvas)
    public ClearTipInputPanel clearTipInput;
    public ClearTipInputPanel ClearTipInput => clearTipInput != null ? clearTipInput : (clearTipInput = GetComponent<ClearTipInputPanel>());

    public override void OpenCanvas(Action closeCallback = null)
    {
        // (2026-11-27) 멀티 판: 싱글 결과·보상 대신 멀티 결과 창 (순위·전적, MultiInGameSession)
        if (MultiGame.IsActive && MultiInGameSession.Instance != null)
        {
            MultiInGameSession.Instance.OnLocalGameEnded(GameManager.Instance.isCleared);
            return;
        }

        base.OpenCanvas(closeCallback);

        isDoubled = false;
        isWatchingAd = false;
        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        string stageKey = GameManager.Instance.stageData.key;
        if (!MultiGame.IsActive)
        {
            LocalGameSave.Clear(); // (2026-11-27) 싱글 판이 끝남 → 이어하기 저장 삭제
            PlayRecorder.Finish(GameManager.Instance.isCleared); // 플레이 기록: 로컬 백업 → 서버
        }
        // 한 판 = 결과 화면까지 (부활해도 같은 판)
        UserDataManager.Instance.userStageManager.TryStage(stageKey);
        GameEventBus.Publish(new TryStageEvent(stageKey));
        AchievementManager.Report(AchievementType.TryStage);

        UserStage userStage = UserDataManager.Instance.userStageManager.GetUserStage(stageKey);
        // 이번 판 기록을 저장하기 전 처치한 보스 수 (로비 보스 처치 연출용, LobbyReturnController)
        int prevDefeatedBossCount = LobbyReturnController.GetDefeatedBossCount(userStage.maxWave, userStage.isClear);
        // (2026-11-24) 제목: 스테이지 클리어(마지막 wave 보스 처치) = STAGE CLEAR!, 아니면 GAME OVER
        if (resultTitleText == null)
            resultTitleText = FindChildText("ResultTitleText");
        if (resultTitleText != null)
            resultTitleText.text = GameManager.Instance.isCleared ? "STAGE CLEAR!" : "GAME OVER";
        // wave 표시 UI가 씬에 없어도 보상은 지급되도록 (없으면 표시만 생략)
        if (curWaveText != null)
            curWaveText.text = $"WAVE {GameManager.Instance.waveIdx + 1}";
        // if (bestWaveText != null)
        //     bestWaveText.text = (Mathf.Max(userStage.maxWave, GameManager.Instance.waveIdx) + 1).ToString();
        if (newRecordObj != null)
            newRecordObj.SetActive(GameManager.Instance.waveIdx > userStage.maxWave);


        // 넘긴 wave 수 = waveIdx (진행 중이던 wave는 제외). 클리어면 마지막 wave까지 모두 넘김
        bool isCleared = GameManager.Instance.isCleared;
        int passedWaveCount = isCleared ? GameManager.Instance.waveIdx + 1 : GameManager.Instance.waveIdx;
        if (isCleared)
        {
            UserDataManager.Instance.userStageManager.ClearStage(stageKey);
            AchievementManager.Report(AchievementType.ClearStage);
        }
        // 보스를 처음 처치했으면 로비로 돌아갈 때 연출
        LobbyReturnController.RecordGameResult(prevDefeatedBossCount, GameManager.Instance.waveIdx, isCleared);
        // (2026-11-26) 클리어면 "클리어 팁 남기기" 입력칸 (로비 전투 화면 하단 띠지에 흐름)
        ClearTipInputPanel tipPanel = ClearTipInput;
        if (tipPanel != null)
        {
            if (isCleared && ClearTipManager.CanWrite(stageKey))
                tipPanel.Show(stageKey);
            else
                tipPanel.Hide();
        }

        // 게임 기록 (최고 기록 = 이번 판 포함, 표시 번호)
        if (statsPanel != null)
            statsPanel.Show(Mathf.Max(userStage.maxWave, GameManager.Instance.waveIdx) + 1);
        else
            Debug.LogWarning("[ResultCanvas] statsPanel 없음 — 메뉴 Tools/InGame/Add Result Stats To ResultCanvas");

        //최대 깊이에 따라서 보상 받게 하자
        // float distance = Vector2.Distance(Character.Instance.transform.position,Vector2.zero); 
        // distanceText.text = distance.ToString();            

        // 최대 레벨이면 경험치 보상 없음 (0으로 유지)
        var myInfo = UserDataManager.Instance.userMyInfoManager;
        rewardExp = UserMyInfoManager.GetGainExp(myInfo.userMyInfoData.lv, passedWaveCount / 2);
        myInfo.AddExp(rewardExp);
        UserMyInfoManager.unshownGainExp += rewardExp; // 로비에서 바가 차오르는 연출

        ShowRewards();
        SoundManager.Play(SFXType.RewardGet);

        UserDataManager.Instance.userStageManager.EndStage(GameManager.Instance.stageData.key, GameManager.Instance.waveIdx); // 저장값 = waveIdx (클리어면 59 = 표시 60)
    }

    TMP_Text FindChildText(string name)
    {
        foreach (var t in GetComponentsInChildren<TMP_Text>(true))
            if (t.name == name)
                return t;
        return null;
    }

    // 보상 패널 표시 (2배 받았으면 2배 값으로). animate = 값이 바뀐 걸 보여주는 글씨 확대 이펙트
    void ShowRewards(bool animate = false)
    {
        int mul = isDoubled ? 2 : 1;
        List<RewardData> rewardDatas = new List<RewardData>();
        if (rewardExp > 0)
            rewardDatas.Add(new RewardData() { rewardType = RewardType.Exp, value = (rewardExp * mul).ToString() });

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

    // 광고 보고 보상 2배 (경험치). 광고 성공 시에만, 광고 제거 구매자는 광고 없이
    public void OnClickedDoubleReward()
    {
        if (isDoubled || isWatchingAd)
            return;
        if (rewardExp <= 0)
            return;

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
        ShowRewards(true);
        SoundManager.Play(SFXType.RewardDouble);
    }

    public void OnClickedHome()
    {
        if (isWatchingAd)
            return;
        CloseCanvas();
        if (MultiGame.IsActive && MultiMatchManager.Exists) // (2026-11-27) 멀티 판을 나감 (멀티 상태 정리 + 로비, 싱글 저장본은 로비 [이어하기])
        {
            MultiMatchManager.Instance.ExitMultiGame();
            return;
        }
        FadeCanvs.Instance.FadeOutIn(null, () => { SceneManager.LoadScene("Lobby"); });
    }
}
