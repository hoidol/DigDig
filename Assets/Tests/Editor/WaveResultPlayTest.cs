using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// 웨이브 진행(엘리트·보스 주기) / 사망 → 결과 화면 → 보상 (CLAUDE.md "웨이브", ResultCanvas)
public class WaveResultPlayTest : InGamePlayTestBase
{
    static Boss FindBoss() => Object.FindObjectsByType<Boss>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.activeInHierarchy);

    [UnityTest]
    public IEnumerator Waves_EliteAndBoss_ByInterval()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Waves_EliteAndBoss_ByIntervalBody();
    }

    IEnumerator Waves_EliteAndBoss_ByIntervalBody()
    {

        var started = new List<(int idx, WaveType type)>();
        System.Action<WaveStartEvent> onStart = e => started.Add((e.waveIdx, e.waveType));
        GameEventBus.Subscribe(onStart);
        try
        {
            int bossIdx = GameSetting.BOSS_WAVE_INTERVAL - 1;
            int waveClearGold = (int)KingSlime.Instance.statMgr.GetValue(StatType.WaveClearGold);

            // 0 → 보스 wave까지 진행
            while (GameManager.Instance.waveIdx < bossIdx)
            {
                int idx = GameManager.Instance.waveIdx;
                current = $"wave {idx}";
                int coin = KingSlime.Instance.coin;
                yield return SkipWave();
                Assert.AreEqual(idx + 1, GameManager.Instance.waveIdx, $"wave {idx}에서 다음 wave로 넘어가지 않음");
                Assert.GreaterOrEqual(KingSlime.Instance.coin - coin, waveClearGold, $"wave {idx} 종료 코인(WaveClearGold {waveClearGold})"); // 처치 코인이 더해질 수 있음
            }

            foreach (var (idx, type) in started)
                Assert.AreEqual(GameManager.GetWaveType(idx), type, $"wave {idx}(번호 {idx + 1}) 타입");
            Assert.IsTrue(started.Any(s => s.type == WaveType.Elite), "엘리트 wave가 없었음");

            // 보스 wave: 보스 소환, 시간이 다 돼도 안 넘어감
            current = $"boss wave {bossIdx}";
            Assert.IsInstanceOf<BossWave>(GameManager.Instance.CurrentWave, $"{bossIdx + 1}번째 wave가 보스 wave가 아님");
            Boss boss = FindBoss();
            Assert.IsNotNull(boss, "보스가 소환되지 않음");
            Assert.AreEqual(WaveType.Boss, started.Last().type, "보스 wave에 엘리트 등 다른 타입이 섞임");
            Assert.IsNotNull(boss.GetAttackPattern("Sweep"), "보스 스킬 Sweep 없음");
            Assert.IsNotNull(boss.GetAttackPattern("RandomDrop"), "보스 스킬 RandomDrop 없음");

            GameManager.Instance.CurrentWave.waveTimer = 0f;
            GameManager.Instance.mainWaveTimer = 0.01f;
            yield return WaitRealtime(1f);
            Assert.AreEqual(bossIdx, GameManager.Instance.waveIdx, "보스 wave가 시간으로 넘어감");
            Assert.IsNotNull(FindBoss(), "보스가 사라짐");

            // 보스 처치 → 종료 코인 + 바로 다음 wave
            int coinBeforeBoss = KingSlime.Instance.coin;
            boss.TakeDamage(KILL);
            yield return WaitRealtime(0.5f);
            Assert.AreEqual(bossIdx + 1, GameManager.Instance.waveIdx, "보스 처치 후 다음 wave로 안 넘어감");
            Assert.AreEqual(WaveType.Battle, GameManager.Instance.CurrentWave.waveType);
            Assert.GreaterOrEqual(KingSlime.Instance.coin - coinBeforeBoss, waveClearGold, "보스 처치 시 wave 종료 코인");
            Assert.IsTrue(GameManager.Instance.isPlaying, "보스 처치 후 게임이 끝남 (무한 웨이브)");
        }
        finally
        {
            GameEventBus.Unsubscribe(onStart);
        }
        AssertNoErrors();
    }

    // 사망 → 실패 화면 → 취소 → 결과 화면 + 보상(골드 = waveIdx, 경험치 = waveIdx/2, 뽑기권 = 합성 횟수) + 기록
    [UnityTest]
    public IEnumerator Death_Result_GivesRewards()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Death_Result_GivesRewardsBody();
    }

    IEnumerator Death_Result_GivesRewardsBody()
    {

        for (int i = 0; i < 6; i++)
            yield return SkipWave();
        int waveIdx = GameManager.Instance.waveIdx;
        Assert.AreEqual(6, waveIdx);
        SlimeFieldController.Instance.mergeCount = 3;

        var user = UserDataManager.Instance;
        string stageKey = GameManager.Instance.stageData.key;
        UserStage stage = user.userStageManager.GetUserStage(stageKey);
        int gold = user.userData.gold;
        int drawTicket = user.userData.drawTicket;
        int exp = user.userMyInfoManager.userMyInfoData.exp;
        int tryCount = stage.tryCount;
        int maxWave = stage.maxWave;

        KingSlime.Instance.TakeDamage(KILL);
        yield return WaitFrames(3);
        Assert.IsFalse(GameManager.Instance.isPlaying, "체력 0인데 게임이 안 끝남");
        Assert.IsTrue(FailCanvas.Instance.gameObject.activeSelf, "실패 화면이 안 열림");
        Assert.AreEqual(tryCount, stage.tryCount, "도전 횟수는 결과 화면에서 올라야 함 (사망 시점 X)");
        Assert.AreEqual(gold, user.userData.gold, "결과 화면 전에 보상이 들어감");

        FailCanvas.Instance.OnClickedCancel();
        yield return WaitFrames(3);
        ResultCanvas result = ResultCanvas.Instance;
        Assert.IsFalse(FailCanvas.Instance.gameObject.activeSelf);
        Assert.IsTrue(result.gameObject.activeSelf, "결과 화면이 안 열림");
        Assert.AreEqual(tryCount + 1, stage.tryCount, "결과 화면에서 도전 횟수 +1");
        Assert.IsNotNull(result.curWaveText, "최종 웨이브 텍스트 연결 안 됨");
        // Assert.IsNotNull(result.bestWaveText, "최고 웨이브 텍스트 연결 안 됨");
        Assert.IsNotNull(result.newRecordObj, "신기록 오브젝트 연결 안 됨");
        Assert.AreEqual((waveIdx + 1).ToString(), result.curWaveText.text, "최종 웨이브 (wave 번호)");
        // Assert.AreEqual((Mathf.Max(maxWave, waveIdx) + 1).ToString(), result.bestWaveText.text, "최고 웨이브 (이번 판 포함)");

        Assert.AreEqual(gold + waveIdx, user.userData.gold, "골드 보상 (= waveIdx)");
        Assert.AreEqual(exp + waveIdx / 2, user.userMyInfoManager.userMyInfoData.exp, "경험치 보상 (= waveIdx / 2)");
        Assert.AreEqual(drawTicket + 3, user.userData.drawTicket, "뽑기권 보상 (= 합성 횟수)");
        Assert.AreEqual(Mathf.Max(maxWave, waveIdx), stage.maxWave, "최고 wave 기록");
        Assert.AreEqual(waveIdx > maxWave, result.newRecordObj.activeSelf, "신기록 표시");

        int shownRewards = result.rewardPanels.Count(p => p.gameObject.activeSelf);
        Assert.AreEqual(3, shownRewards, "보상 패널 수 (골드·경험치·뽑기권)");

        // 광고 보고 2배: 같은 양을 한 번 더 지급 (한 번만)
        if (result.adDoubleRewardButton != null)
            Assert.IsTrue(result.adDoubleRewardButton.activeSelf, "2배 보상 버튼이 안 보임");
        result.OnClickedDoubleReward();
        yield return WaitFrames(3);
        Assert.IsTrue(result.IsDoubled, "2배 보상이 적용 안 됨");
        Assert.AreEqual(gold + waveIdx * 2, user.userData.gold, "골드 2배");
        Assert.AreEqual(exp + waveIdx / 2 * 2, user.userMyInfoManager.userMyInfoData.exp, "경험치 2배");
        Assert.AreEqual(drawTicket + 3 * 2, user.userData.drawTicket, "뽑기권 2배");
        if (result.adDoubleRewardButton != null)
            Assert.IsFalse(result.adDoubleRewardButton.activeSelf, "2배 받은 뒤 버튼 숨김");
        // 글씨 확대 이펙트가 끝나면 2배 값 표시 + 원래 크기
        double popEnd = UnityEditor.EditorApplication.timeSinceStartup + RewardPanel.POP_UP_TIME + RewardPanel.POP_DOWN_TIME + 0.5f;
        while (UnityEditor.EditorApplication.timeSinceStartup < popEnd)
            yield return WaitFrames(1);
        var shownValues = result.rewardPanels.Where(p => p.gameObject.activeSelf).Select(p => p.rewardValueText.text).ToList();
        CollectionAssert.Contains(shownValues, (waveIdx * 2).ToString(), "골드 패널 2배 값 표시");
        CollectionAssert.Contains(shownValues, (3 * 2).ToString(), "뽑기권 패널 2배 값 표시");
        foreach (var p in result.rewardPanels.Where(p => p.gameObject.activeSelf))
            Assert.AreEqual(1f, p.rewardValueText.transform.localScale.x, 0.01f, "이펙트 후 원래 크기");
        result.OnClickedDoubleReward();
        yield return WaitFrames(3);
        Assert.AreEqual(gold + waveIdx * 2, user.userData.gold, "2배는 한 번만");

        // 홈 → 로비 씬
        result.OnClickedHome();
        double end = UnityEditor.EditorApplication.timeSinceStartup + 10;
        while (SceneManager.GetActiveScene().name != "Lobby" && UnityEditor.EditorApplication.timeSinceStartup < end)
            yield return WaitFrames(1);
        Assert.AreEqual("Lobby", SceneManager.GetActiveScene().name, "홈 버튼으로 로비에 안 감");
        yield return WaitFrames(30);
        AssertNoErrors();
    }

    // 부활: 필드 적 제거 → 체력 가득 → 마지막으로 종료된 wave의 다음 wave부터
    [UnityTest]
    public IEnumerator Death_Resume_RestartsFromNextOfClearedWave()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Death_Resume_RestartsFromNextOfClearedWaveBody();
    }

    IEnumerator Death_Resume_RestartsFromNextOfClearedWaveBody()
    {

        for (int i = 0; i < 3; i++)
            yield return SkipWave();
        // wave 3 진행 중, 자기 타이머가 안 끝난 채로 다음 wave 시작 (스킵) → wave 4 진행 중 사망
        GameManager.Instance.StartNextWave();
        yield return WaitRealtime(1f);
        Assert.AreEqual(4, GameManager.Instance.waveIdx);
        int lastCleared = GameManager.Instance.clearWaveIdxes.Max();
        Assert.AreEqual(2, lastCleared);

        PlayerPrefs.DeleteKey(FailCanvas.DAILY_RESURRECTION_DATE_KEY); // 오늘 처음 → 가득
        UserStage stage = UserDataManager.Instance.userStageManager.GetUserStage(GameManager.Instance.stageData.key);
        int tryCount = stage.tryCount;
        int gold = UserDataManager.Instance.userData.gold;
        KingSlime.Instance.TakeDamage(KILL);
        yield return WaitFrames(3);
        Assert.IsTrue(FailCanvas.Instance.gameObject.activeSelf);
        Assert.AreEqual($"일일 부활 {FailCanvas.MAX_DAILY_RESURRECTION_COUNT}", FailCanvas.Instance.leftResurrectionText.text, "처음엔 5회");
        Assert.IsTrue(FailCanvas.Instance.adRetryButton.activeSelf, "부활 버튼이 안 보임");

        FailCanvas.Instance.OnClickeResume(); // 광고(AdManager 없음 → 성공 처리) 후 부활
        yield return WaitFrames(3);
        Assert.AreEqual(FailCanvas.MAX_DAILY_RESURRECTION_COUNT - 1, FailCanvas.LeftResurrectionCount, "부활 횟수 차감");
        Assert.AreEqual(tryCount, stage.tryCount, "부활했는데 도전 횟수가 오름");
        Assert.IsTrue(GameManager.Instance.isPlaying, "부활 후 게임이 재개되지 않음");
        Assert.IsFalse(FailCanvas.Instance.gameObject.activeSelf);
        Assert.AreEqual(KingSlime.Instance.health.MaxHp, KingSlime.Instance.CurHp, 0.01f, "부활 후 체력 가득");
        Assert.AreEqual(lastCleared + 1, GameManager.Instance.waveIdx, "마지막으로 종료된 wave의 다음 wave부터");
        Assert.IsFalse(GameManager.Instance.accWaves.Any(w => w != null && w.waveIdx > lastCleared + 1), "이후 wave가 남아 있음");
        Assert.AreEqual(gold, UserDataManager.Instance.userData.gold, "부활했는데 보상이 들어감");
        yield return WaitRealtime(2f);

        // 다시 사망 → 결과 화면: 한 판으로 도전 횟수 +1
        KingSlime.Instance.TakeDamage(KILL);
        yield return WaitFrames(3);
        Assert.AreEqual($"일일 부활 {FailCanvas.MAX_DAILY_RESURRECTION_COUNT - 1}", FailCanvas.Instance.leftResurrectionText.text);
        FailCanvas.Instance.OnClickedCancel();
        yield return WaitFrames(3);
        Assert.AreEqual(tryCount + 1, stage.tryCount, "부활 포함 한 판 = 도전 1회");
        AssertNoErrors();
    }

    // 일일 부활 5회 제한: 다 쓰면 버튼 숨김·눌러도 부활 안 함, 날짜가 바뀌면 다시 5회
    [UnityTest]
    public IEnumerator DailyResurrection_LimitAndReset()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return DailyResurrection_LimitAndResetBody();
    }

    IEnumerator DailyResurrection_LimitAndResetBody()
    {
        PlayerPrefs.SetString(FailCanvas.DAILY_RESURRECTION_DATE_KEY, System.DateTime.Now.ToString("yyyyMMdd"));
        PlayerPrefs.SetInt(FailCanvas.DAILY_RESURRECTION_COUNT_KEY, 0);

        KingSlime.Instance.TakeDamage(KILL);
        yield return WaitFrames(3);
        Assert.AreEqual("일일 부활 0", FailCanvas.Instance.leftResurrectionText.text);
        Assert.IsFalse(FailCanvas.Instance.adRetryButton.activeSelf, "횟수를 다 썼는데 부활 버튼이 보임");
        FailCanvas.Instance.OnClickeResume();
        yield return WaitFrames(3);
        Assert.IsFalse(GameManager.Instance.isPlaying, "횟수를 다 썼는데 부활함");
        Assert.IsTrue(FailCanvas.Instance.gameObject.activeSelf);

        // 어제 기록이면 오늘은 다시 가득
        PlayerPrefs.SetString(FailCanvas.DAILY_RESURRECTION_DATE_KEY, System.DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
        Assert.AreEqual(FailCanvas.MAX_DAILY_RESURRECTION_COUNT, FailCanvas.LeftResurrectionCount, "날짜가 바뀌면 5회로 초기화");
        AssertNoErrors();
    }
}
