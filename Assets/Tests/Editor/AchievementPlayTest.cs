using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// 업적 (CLAUDE.md "업적"): CSV대로 업적 창 구성, 집계 → 진행도·상태 표시 → 보상 수령, 날짜가 바뀌면 일일 초기화 + Login
// 로비: 업적 창 UI 전체 흐름 / 인게임: 실제 행동(소환 버튼·합성·아이템 뽑기·강화·처치)이 집계되는지
public class AchievementPlayTest : InGamePlayTestBase
{
    static AchievementManager Mgr => AchievementManager.Instance;
    static DailyAchievementManager Daily => Mgr.dailyAchievementManager;
    static CumulativeAchievementManager Cumulative => Mgr.cumulativeAchievementManager;
    static UserData User => UserDataManager.Instance.userData;

    static int DailyValue(AchievementType type) => Daily.userDailyAchievementManager.GetUserAchievement(type).value;
    static int CumulativeValue(AchievementType type) => Cumulative.userCumulativeAchievementManager.GetUserAchievement(type).value;
    static UserCumulativeAchievement CumulativeUser(AchievementType type) => Cumulative.userCumulativeAchievementManager.GetUserAchievement(type);

    [TearDown]
    public void ResetDate() => DailyCheck.NowForTest = null;

    // 기록을 비우고 고정된 날짜로 시작 (날짜가 바뀐 것으로 처리돼 Login 1)
    static void ResetAchievements(DateTime now)
    {
        DailyCheck.NowForTest = now;
        Daily.userDailyAchievementManager.userDailyAchievementData.userDailyAchievements.Clear();
        Daily.userDailyAchievementManager.userDailyAchievementData.date = "";
        Cumulative.userCumulativeAchievementManager.userCumulativeAchievementData.userCumulativeAchievements.Clear();
        Mgr.CheckNewDay();
    }

    static AchievementPanel[] ActivePanels(AchievementContainer container)
        => container.GetComponentsInChildren<AchievementPanel>(true).Where(p => p.gameObject.activeSelf).ToArray();

    static T Panel<T>(AchievementType type) where T : AchievementPanel
        => AchievementCanvas.Instance.GetComponentsInChildren<T>(true).First(p => p.gameObject.activeSelf && p.achievementData != null && p.achievementData.type == type);

    static void ClickClaim(AchievementPanel panel)
    {
        Button button = panel.claimObj.GetComponentInChildren<Button>(true);
        Assert.IsNotNull(button, $"{panel.name}: claimObj에 Button 없음");
        button.onClick.Invoke();
    }

    // 패널 표시가 데이터와 맞는지: 이름·진행 글씨·슬라이더·보상·상태 오브젝트 하나만
    static void AssertPanel(AchievementPanel panel, string step)
    {
        AchievementData data = panel.achievementData;
        string who = $"[{step}] {panel.GetType().Name} {data.type}";
        Assert.IsFalse(panel.titleText.text.StartsWith("*"), $"{who}: 이름 번역 없음 ({panel.titleText.text})");
        Assert.AreEqual(TranslateManager.GetText($"Achievement.{data.type}"), panel.titleText.text, $"{who}: 이름");
        Assert.AreEqual(data.rewardData.value, panel.rewardValueText.text, $"{who}: 보상 수치");
        int value = data.GetUserAchievement().value;
        int goal = data.GetGoal();
        Assert.AreEqual(Mathf.Clamp01((float)value / goal), panel.slider.value, 0.001f, $"{who}: 슬라이더 ({value}/{goal})");

        bool canClaim;
        if (panel is DailyAchievementPanel daily)
        {
            var user = data.GetUserAchievement() as UserDailyAchievement;
            canClaim = !user.getReward && value >= goal;
            Assert.AreEqual($"{Mathf.Min(value, goal)}/{goal}", panel.progressText.text, $"{who}: 진행 글씨");
            Assert.AreEqual(user.getReward, daily.gottenRewardObj.activeSelf, $"{who}: gottenRewardObj (보상 받음)");
            Assert.AreEqual(!user.getReward && !canClaim, daily.processingObj.activeSelf, $"{who}: processingObj (진행 중)");
        }
        else
        {
            var cumulative = panel as CumulativeAchievementPanel;
            canClaim = value >= goal;
            Assert.AreEqual($"{value}/{goal}", panel.progressText.text, $"{who}: 진행 글씨");
            Assert.AreEqual(!canClaim, cumulative.processingObj.activeSelf, $"{who}: processingObj (진행 중)");
        }
        Assert.AreEqual(canClaim, panel.claimObj.activeSelf, $"{who}: claimObj (받을 수 있음)");
        Assert.AreEqual(canClaim, panel.redDot.activeSelf, $"{who}: redDot");
    }

    static void AssertAllPanels(string step)
    {
        foreach (AchievementContainer container in AchievementCanvas.Instance.achievementContainers)
            foreach (AchievementPanel panel in ActivePanels(container))
                AssertPanel(panel, step);
    }

    [UnityTest]
    public IEnumerator Lobby_AchievementCanvas()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return LobbyBody();
    }

    IEnumerator LobbyBody()
    {
        Assert.IsNotNull(Mgr, "AchievementManager 없음");
        Assert.AreEqual(5, Daily.dailyAchievementDatas.Length, "DailyAchievement.csv 행 수");
        Assert.AreEqual(11, Cumulative.cumulativeAchievementDatas.Length, "CumulativeAchievement.csv 행 수");

        DateTime day1 = new DateTime(2030, 1, 10, 9, 0, 0);
        ResetAchievements(day1);
        Assert.AreEqual(1, CumulativeValue(AchievementType.Login), "날짜가 바뀐 첫 접속 = Login 1");
        Mgr.CheckNewDay();
        Assert.AreEqual(1, CumulativeValue(AchievementType.Login), "같은 날 다시 확인해도 Login은 그대로");

        // 1) 업적 버튼 → 창. 일일 탭이 먼저, 패널 수 = CSV 행 수 (누적은 씬 패널이 모자라면 코드가 복사)
        AchievementButton openButton = UnityEngine.Object.FindFirstObjectByType<AchievementButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(openButton, "로비에 AchievementButton 없음");
        openButton.UpdateButton();
        Assert.IsTrue(openButton.redDot.activeSelf, "Login 누적(1/1)을 받을 수 있는데 업적 버튼 redDot이 꺼짐");
        openButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(3);

        AchievementCanvas canvas = AchievementCanvas.Instance;
        Assert.IsTrue(canvas.gameObject.activeInHierarchy, "업적 버튼을 눌러도 창이 안 열림");
        var dailyContainer = canvas.GetComponentInChildren<DailyAchievementContainer>(true);
        var cumulativeContainer = canvas.GetComponentInChildren<CumulativeAchievementContainer>(true);
        Assert.Contains(dailyContainer, canvas.achievementContainers, "achievementContainers에 일일 목록 없음");
        Assert.Contains(cumulativeContainer, canvas.achievementContainers, "achievementContainers에 누적 목록 없음");
        Assert.IsTrue(dailyContainer.gameObject.activeInHierarchy, "처음엔 일일 탭");
        Assert.IsFalse(cumulativeContainer.gameObject.activeInHierarchy, "처음엔 누적 탭이 꺼져 있어야 함");

        Assert.AreEqual(Daily.dailyAchievementDatas.Select(d => d.type).ToArray(),
            ActivePanels(dailyContainer).Select(p => p.achievementData.type).ToArray(), "일일 패널 = CSV 순서");
        Assert.AreEqual(Cumulative.cumulativeAchievementDatas.Select(d => d.type).ToArray(),
            ActivePanels(cumulativeContainer).Select(p => p.achievementData.type).ToArray(), "누적 패널 = CSV 순서");
        AssertAllPanels("처음");
        Assert.IsTrue(ActivePanels(dailyContainer).All(p => ((DailyAchievementPanel)p).processingObj.activeSelf), "기록이 없으면 일일은 모두 진행 중");

        // 탭 전환: 버튼 2개, 누적 탭 redDot (Login)
        Assert.AreEqual(2, canvas.categoryTapButtons.Length, "탭 버튼 수");
        Assert.IsTrue(canvas.categoryTapButtons.All(b => b != null), "categoryTapButtons에 빈 칸");
        var dailyTap = canvas.categoryTapButtons.First(b => b.category == AchievementCategory.Daily);
        var cumulativeTap = canvas.categoryTapButtons.First(b => b.category == AchievementCategory.Cumulative);
        Assert.IsFalse(dailyTap.redDot.activeSelf, "받을 일일 업적이 없는데 일일 탭 redDot");
        Assert.IsTrue(cumulativeTap.redDot.activeSelf, "Login을 받을 수 있는데 누적 탭 redDot이 꺼짐");
        cumulativeTap.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsTrue(cumulativeContainer.gameObject.activeInHierarchy, "누적 탭 버튼을 눌러도 누적 목록이 안 보임");
        Assert.IsFalse(dailyContainer.gameObject.activeInHierarchy, "누적 탭인데 일일 목록이 보임");
        dailyTap.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsTrue(dailyContainer.gameObject.activeInHierarchy, "일일 탭으로 돌아오지 않음");

        // 2) 일일: 진행 → 받을 수 있음 → 보상 → 완료
        var dailyKill = Panel<DailyAchievementPanel>(AchievementType.KillEnemy);
        int dailyGoal = dailyKill.achievementData.GetGoal();
        AchievementManager.Report(AchievementType.KillEnemy, dailyGoal - 1);
        yield return WaitFrames(2);
        Assert.AreEqual($"{dailyGoal - 1}/{dailyGoal}", dailyKill.progressText.text, "집계하면 열려 있는 창이 바로 갱신돼야 함 (UpdateAchievementEvent)");
        AssertAllPanels("일일 목표 1 모자람");
        int gold = User.gold;
        ClickClaim(dailyKill);
        Assert.AreEqual(gold, User.gold, "목표 미달인데 보상 지급");

        AchievementManager.Report(AchievementType.KillEnemy, 200); // 목표 초과
        yield return WaitFrames(2);
        Assert.AreEqual($"{dailyGoal}/{dailyGoal}", dailyKill.progressText.text, "일일 진행 글씨는 목표를 넘지 않음");
        Assert.IsTrue(dailyKill.claimObj.activeSelf && !dailyKill.processingObj.activeSelf && !dailyKill.gottenRewardObj.activeSelf, "받을 수 있음 = claimObj만");
        Assert.IsTrue(dailyTap.redDot.activeSelf, "일일 탭 redDot");
        AssertAllPanels("일일 달성");

        int reward = int.Parse(dailyKill.achievementData.rewardData.value);
        ClickClaim(dailyKill);
        yield return WaitFrames(2);
        Assert.AreEqual(gold + reward, User.gold, "일일 보상 골드");
        Assert.IsTrue(dailyKill.gottenRewardObj.activeSelf && !dailyKill.claimObj.activeSelf && !dailyKill.processingObj.activeSelf, "보상 받음 = gottenRewardObj만");
        Assert.IsFalse(dailyTap.redDot.activeSelf, "받고 나면 일일 탭 redDot 꺼짐");
        ClickClaim(dailyKill);
        Mgr.dailyAchievementManager.GetReward(dailyKill.achievementData);
        Assert.AreEqual(gold + reward, User.gold, "일일 보상은 하루 한 번");
        var goldPanels = UnityEngine.Object.FindObjectsByType<CurrencyPanel>(FindObjectsSortMode.None).Where(p => p.currencyType == CurrencyType.Gold && p.isActiveAndEnabled).ToArray();
        foreach (CurrencyPanel p in goldPanels)
            Assert.AreEqual(User.gold.ToString(), p.currencyText.text, $"보상 후 골드 표시 ({p.name})");
        AssertAllPanels("일일 수령");

        // 3) 누적: 목표 = InitGoal + IncreaseGoal × 클리어 횟수, 초과분 이월, 받으면 다음 목표
        cumulativeTap.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(2);
        var cumKill = Panel<CumulativeAchievementPanel>(AchievementType.KillEnemy);
        var killData = cumKill.achievementData as CumulativeAchievementData;
        int total = dailyGoal - 1 + 200;
        Assert.AreEqual(total, CumulativeValue(AchievementType.KillEnemy), "누적도 같이 집계");
        Assert.AreEqual($"{total}/{killData.initGoal}", cumKill.progressText.text);
        gold = User.gold;
        int cumReward = int.Parse(killData.rewardData.value);
        int expectedValue = total;
        for (int clear = 0; clear < 3; clear++)
        {
            int goal = killData.initGoal + killData.increaseGoal * clear;
            Assert.AreEqual(goal, killData.GetGoal(), $"클리어 {clear}회 목표");
            Assert.IsTrue(cumKill.claimObj.activeSelf, $"클리어 {clear}회: 받을 수 있어야 함 ({expectedValue}/{goal})");
            ClickClaim(cumKill);
            yield return WaitFrames(2);
            expectedValue -= goal;
            Assert.AreEqual(gold + cumReward * (clear + 1), User.gold, $"누적 보상 {clear + 1}번째");
            Assert.AreEqual(expectedValue, CumulativeValue(AchievementType.KillEnemy), "받으면 목표만큼 빼고 나머지는 이월");
            Assert.AreEqual(clear + 1, CumulativeUser(AchievementType.KillEnemy).clearCount);
            AssertPanel(cumKill, $"누적 {clear + 1}회 수령");
        }

        // Login 1/1 → 받으면 0/2
        var login = Panel<CumulativeAchievementPanel>(AchievementType.Login);
        Assert.IsTrue(login.claimObj.activeSelf, "Login 1/1 받을 수 있음");
        ClickClaim(login);
        yield return WaitFrames(2);
        Assert.AreEqual("0/2", login.progressText.text, "Login 다음 목표");
        Assert.IsTrue(login.processingObj.activeSelf && !login.claimObj.activeSelf);

        // 4) 광고: 성공할 때마다 Watch_Ad +1 (광고 제거 구매자도). 목표는 IncreaseGoal 0이라 계속 2, 보상 다이아
        var ad = Panel<CumulativeAchievementPanel>(AchievementType.Watch_Ad);
        bool wasAdRemoved = UserDataManager.Instance.isAdRemoved;
        UserDataManager.Instance.isAdRemoved = false;
        bool adResult = false;
        AdManager.Show("test", AdType.RA, s => adResult = s);
        Assert.IsTrue(adResult);
        Assert.AreEqual(1, CumulativeValue(AchievementType.Watch_Ad), "광고 성공 → Watch_Ad 1");
        UserDataManager.Instance.isAdRemoved = true;
        adResult = false;
        AdManager.Show("test", AdType.RA, s => adResult = s);
        Assert.IsTrue(adResult, "광고 제거 구매자는 광고 없이 성공");
        Assert.AreEqual(2, CumulativeValue(AchievementType.Watch_Ad), "광고 제거 구매자도 Watch_Ad 집계");
        UserDataManager.Instance.isAdRemoved = wasAdRemoved;
        yield return WaitFrames(2);
        int dia = User.dia;
        ClickClaim(ad);
        yield return WaitFrames(2);
        Assert.AreEqual(dia + int.Parse(ad.achievementData.rewardData.value), User.dia, "Watch_Ad 보상 다이아");
        Assert.AreEqual(2, ad.achievementData.GetGoal(), "IncreaseGoal 0 → 목표 그대로");
        Assert.AreEqual("0/2", ad.progressText.text);

        // 5) 로비 행동 집계: 슬라임 뽑기 = 뽑은 장수, 슬라임 강화 = 1
        var drawPanel = UnityEngine.Object.FindFirstObjectByType<DrawMinionProductPanel>(FindObjectsInactive.Include);
        Assert.IsNotNull(drawPanel, "로비에 DrawMinionProductPanel 없음");
        int drawBefore = CumulativeValue(AchievementType.DrawMinion);
        drawPanel.Purchased(10);
        yield return WaitFrames(2);
        Assert.AreEqual(drawBefore + 10, CumulativeValue(AchievementType.DrawMinion), "10회 뽑기 = DrawMinion 10");
        if (Lobby.DrawMinionResultCanvas.Instance != null)
            Lobby.DrawMinionResultCanvas.Instance.CloseCanvas();

        int levelUpBefore = CumulativeValue(AchievementType.LevelUpMinion);
        UserDataManager.Instance.userMinionManager.LevelUp(UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[0].key);
        Assert.AreEqual(levelUpBefore + 1, CumulativeValue(AchievementType.LevelUpMinion), "슬라임 강화 = LevelUpMinion 1");
        yield return WaitFrames(2);
        AssertAllPanels("로비 행동 후");

        // 6) 저장 파일 (집계는 프레임 끝에 한 번 저장)
        var savedDaily = SaveManager.LoadData<UserDailyAchievementData>("UserDailyAchievement");
        Assert.AreEqual("20300110", savedDaily.date, "저장된 일일 업적 날짜");
        Assert.IsTrue(savedDaily.userDailyAchievements.First(a => a.type == AchievementType.KillEnemy).getReward, "저장: 일일 수령 여부");
        var savedCumulative = SaveManager.LoadData<UserCumulativeAchievementData>("UserCumulativeAchievement");
        Assert.AreEqual(3, savedCumulative.userCumulativeAchievements.First(a => a.type == AchievementType.KillEnemy).clearCount, "저장: 누적 클리어 횟수");
        Assert.AreEqual(drawBefore + 10, savedCumulative.userCumulativeAchievements.First(a => a.type == AchievementType.DrawMinion).value, "저장: 누적 값");

        // 7) 날짜가 바뀜 → 창을 열 때 일일 초기화(진행도·수령 여부), Login +1, 누적은 유지
        canvas.CloseCanvas();
        int cumKillValue = CumulativeValue(AchievementType.KillEnemy);
        DailyCheck.NowForTest = day1.AddDays(1);
        openButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(3);
        Assert.AreEqual(0, DailyValue(AchievementType.KillEnemy), "다음 날: 일일 진행도 초기화");
        Assert.IsFalse(Daily.userDailyAchievementManager.GetUserAchievement(AchievementType.KillEnemy).getReward, "다음 날: 일일 수령 여부 초기화");
        dailyKill = Panel<DailyAchievementPanel>(AchievementType.KillEnemy);
        Assert.AreEqual($"0/{dailyGoal}", dailyKill.progressText.text, "다음 날: 일일 패널 표시");
        Assert.IsTrue(dailyKill.processingObj.activeSelf && !dailyKill.gottenRewardObj.activeSelf && !dailyKill.claimObj.activeSelf, "다음 날: 일일은 다시 진행 중");
        Assert.AreEqual(1, CumulativeValue(AchievementType.Login), "다음 날 접속 = Login +1 (0 → 1)");
        Assert.AreEqual(cumKillValue, CumulativeValue(AchievementType.KillEnemy), "누적은 날짜가 바뀌어도 유지");
        Assert.AreEqual(3, CumulativeUser(AchievementType.KillEnemy).clearCount);
        AssertAllPanels("다음 날");

        // 창을 연 채로 자정이 지나도 다음 집계 때 초기화
        AchievementManager.Report(AchievementType.KillEnemy, 7);
        Assert.AreEqual(7, DailyValue(AchievementType.KillEnemy));
        DailyCheck.NowForTest = day1.AddDays(2).AddHours(-9); // 다음 날 0시
        AchievementManager.Report(AchievementType.KillEnemy, 2);
        Assert.AreEqual(2, DailyValue(AchievementType.KillEnemy), "자정이 지난 뒤 첫 집계 = 초기화 후 그 값만");
        Assert.AreEqual(2, CumulativeValue(AchievementType.Login), "날짜가 또 바뀜 → Login +1");
        yield return WaitFrames(2);
        AssertAllPanels("자정 지난 뒤");

        canvas.CloseCanvas();
        yield return WaitFrames(2);
        AssertNoErrors();
    }

    [UnityTest]
    public IEnumerator InGame_ActionsAreCounted()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return InGameBody();
    }

    static readonly MethodInfo DROP = typeof(MinionFieldController).GetMethod("Merge",
        BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Minion), typeof(Minion), typeof(Vector3) }, null);

    IEnumerator InGameBody()
    {
        Assert.IsNotNull(Mgr, "인게임에 AchievementManager 없음");
        ResetAchievements(new DateTime(2030, 1, 10, 9, 0, 0));
        Time.timeScale = 0f; // 적이 저절로 죽거나 킹이 맞지 않게 멈추고 행동만 확인

        // 소환 버튼 = SpawnMinion 1 (일일·누적 모두), 코인이 모자라면 집계 없음
        var spawnButton = UnityEngine.Object.FindFirstObjectByType<MinionSpawnButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(spawnButton, "MinionSpawnButton 없음");
        KingSlime.Instance.AddCoin(-KingSlime.Instance.coin);
        spawnButton.OnClickedBtn();
        Assert.AreEqual(0, CumulativeValue(AchievementType.SpawnMinion), "코인 부족으로 소환 실패인데 집계됨");
        KingSlime.Instance.AddCoin(100000);
        spawnButton.OnClickedBtn();
        spawnButton.OnClickedBtn();
        Assert.AreEqual(2, CumulativeValue(AchievementType.SpawnMinion), "소환 버튼 2번");
        Assert.AreEqual(2, DailyValue(AchievementType.SpawnMinion), "일일도 같이 집계");

        // 버튼이 아닌 소환(무료 소환 등)은 제외
        Minion a = SpawnMinion("Iron", GetFreeTile(), 0);
        Minion b = SpawnMinion("Iron", GetFreeTile(), 0);
        Assert.AreEqual(2, CumulativeValue(AchievementType.SpawnMinion), "버튼이 아닌 소환이 집계됨");

        // 합성 = MergeMinion 1, 합성으로 생긴 슬라임은 소환으로 안 셈
        Assert.IsNotNull(DROP, "MinionFieldController.Merge(source, target, originPos) 없음");
        DROP.Invoke(MinionFieldController.Instance, new object[] { a, b, a.transform.position });
        yield return WaitFrames(3);
        Assert.AreEqual(1, CumulativeValue(AchievementType.MergeMinion), "합성 1번");
        Assert.AreEqual(1, DailyValue(AchievementType.MergeMinion));
        Assert.AreEqual(2, CumulativeValue(AchievementType.SpawnMinion), "합성 결과가 소환으로 집계됨");

        // 자리 교체(합성 안 됨)는 제외
        Minion c = SpawnMinion("Iron", GetFreeTile(), 0);
        Minion d = SpawnMinion("Bounce", GetFreeTile(), 0);
        DROP.Invoke(MinionFieldController.Instance, new object[] { c, d, c.transform.position });
        yield return WaitFrames(3);
        Assert.AreEqual(1, CumulativeValue(AchievementType.MergeMinion), "자리 교체가 합성으로 집계됨");

        // 인게임 강화 = UpgradeReinforce 1, 코인 부족이면 제외
        var reinforcePanel = UnityEngine.Object.FindFirstObjectByType<ReinforcePanel>(FindObjectsInactive.Include);
        if (reinforcePanel == null)
            reinforcePanel = new GameObject("ReinforcePanel(Test)", typeof(RectTransform)).AddComponent<ReinforcePanel>();
        ReinforceCanvas.Instance.OpenCanvas();
        yield return WaitFrames(1);
        reinforcePanel.SetMinionData(MinionManager.Instance.GetMinionData(UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[0].key));
        KingSlime.Instance.AddCoin(-KingSlime.Instance.coin);
        reinforcePanel.OnClickReinforce();
        Assert.AreEqual(0, CumulativeValue(AchievementType.UpgradeReinforce), "코인 부족인데 강화 집계");
        KingSlime.Instance.AddCoin(100000);
        reinforcePanel.OnClickReinforce();
        Assert.AreEqual(1, CumulativeValue(AchievementType.UpgradeReinforce), "인게임 강화 1번");
        ReinforceCanvas.Instance.CloseCanvas();

        // 타일 아이템 뽑기 = 시도마다 DrawItem 1 (확률 실패 포함)
        var drawButton = UnityEngine.Object.FindFirstObjectByType<DrawItemButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(drawButton, "DrawItemButton 없음");
        KingSlime.Instance.AddCoin(-KingSlime.Instance.coin);
        drawButton.OnClickedBtn();
        Assert.AreEqual(0, CumulativeValue(AchievementType.DrawItem), "코인 부족인데 뽑기 집계");
        KingSlime.Instance.AddCoin(100000);
        drawButton.OnClickedBtn();
        Assert.AreEqual(1, CumulativeValue(AchievementType.DrawItem), "아이템 뽑기 시도 1번");
        Assert.AreEqual(1, DailyValue(AchievementType.DrawItem));

        // 적 처치 = KillEnemy 1 (시간을 다시 흘려 적이 나오면 처치)
        Time.timeScale = 1f;
        double timeout = UnityEditor.EditorApplication.timeSinceStartup + 20;
        while (EnemyFieldController.Instance.activeEnemies.Count == 0)
        {
            Assert.Less(UnityEditor.EditorApplication.timeSinceStartup, timeout, "적이 나오지 않음");
            yield return WaitRealtime(0f);
        }
        Time.timeScale = 0f;
        int killBefore = CumulativeValue(AchievementType.KillEnemy);
        int dailyKillBefore = DailyValue(AchievementType.KillEnemy);
        Enemy enemy = EnemyFieldController.Instance.activeEnemies.First(e => e.enemyType != EnemyType.BonusElite);
        enemy.TakeDamage(KILL);
        Assert.AreEqual(killBefore + 1, CumulativeValue(AchievementType.KillEnemy), "적 처치 1");
        Assert.AreEqual(dailyKillBefore + 1, DailyValue(AchievementType.KillEnemy));
        Assert.AreEqual(0, CumulativeValue(AchievementType.KillBonusElite), "일반 적이 보너스 엘리트로 집계됨");

        // 보너스 엘리트 처치 = KillEnemy 1 + KillBonusElite 1
        Enemy bonusPrefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.BonusElite);
        Assert.IsNotNull(bonusPrefab, "보너스 엘리트 프리팹 없음");
        EliteEnemy bonus = EnemyFieldController.Instance.Instantiate(bonusPrefab) as EliteEnemy;
        bonus.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), 0);
        yield return WaitFrames(1);
        killBefore = CumulativeValue(AchievementType.KillEnemy);
        bonus.TakeDamage(KILL);
        Assert.AreEqual(1, CumulativeValue(AchievementType.KillBonusElite), "보너스 엘리트 처치 1");
        Assert.AreEqual(killBefore + 1, CumulativeValue(AchievementType.KillEnemy), "보너스 엘리트도 적 처치로 셈");

        // 저장 파일 (프레임 끝에 한 번 저장)
        yield return WaitFrames(3);
        var saved = SaveManager.LoadData<UserCumulativeAchievementData>("UserCumulativeAchievement");
        Assert.AreEqual(2, saved.userCumulativeAchievements.First(x => x.type == AchievementType.SpawnMinion).value, "저장: SpawnMinion");
        Assert.AreEqual(1, saved.userCumulativeAchievements.First(x => x.type == AchievementType.MergeMinion).value, "저장: MergeMinion");
        Assert.AreEqual(1, saved.userCumulativeAchievements.First(x => x.type == AchievementType.KillBonusElite).value, "저장: KillBonusElite");
        Time.timeScale = 1f;
        AssertNoErrors();
    }
}
