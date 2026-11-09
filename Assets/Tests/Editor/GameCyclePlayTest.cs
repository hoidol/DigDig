using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// 게임 한 사이클을 실제 버튼 흐름으로: 로비(KS 업그레이드) → 전투 버튼 → 인게임(부활 성공 / 포기) → 결과 → 로비(레벨업 창)
// → 다시 업그레이드 → 인게임(바로 사망) → 로비. 단계마다 UserDataManager 메모리 값과 저장 파일(Assets/User*.json)을 함께 확인한다.
public class GameCyclePlayTest : InGamePlayTestBase
{
    const int FIRST_RUN_WAVE = 6;                        // 첫 판 사망 시 waveIdx → 골드 6, 경험치 3
    const int FIRST_RUN_EXP = FIRST_RUN_WAVE / 2;
    const int RESUME_AT_WAVE = 3;                        // 이 wave에서 한 번 죽고 부활
    const int FIRST_RUN_MERGE = 2;                       // 뽑기권 보상
    const float EPSILON = 0.0001f;
    const float SCENE_TIMEOUT = 60f;

    static UserDataManager User => UserDataManager.Instance;
    static UserMyInfoManager MyInfo => User.userMyInfoManager;
    static UserKingSlimeManager KSUser => User.userKingSlimeManager;
    static KingSlimeData KSData => KingSlimeManager.Instance.GetKingSlimeData(User.userData.CharacterName);

    // 업그레이드 대상: 체력(인게임에서 바로 확인 가능). 데이터에 없으면 첫 행
    static KSUpgradeInfo UpgradeInfo =>
        KSUpgradeData.Instance.upgradeInfos.FirstOrDefault(i => i.statType == StatType.MaxHp) ?? KSUpgradeData.Instance.upgradeInfos[0];

    static float ExpectedStat(StatType statType)
    {
        float baseValue = KSData.GetKingSlimeStat(statType)?.value ?? 0f;
        KSUpgradeInfo info = KSUpgradeData.Instance.GetUpgradeInfo(statType);
        float upgradeValue = info == null ? 0f : info.valuePerLv * KSUser.GetUpgrade(statType).lv;
        return baseValue + upgradeValue;
    }

    static T Saved<T>(string fileName) => SaveManager.LoadData<T>(fileName);
    static UserData SavedUserData => Saved<UserData>(UserDataManager.UserDataFileName);
    static UserMyInfoData SavedMyInfo => Saved<UserMyInfoData>(UserMyInfoManager.UserDataFileName);
    static UserStage SavedStage(string key) => Saved<UserStageData>(UserStageManager.UserDataFileName).userStages.FirstOrDefault(s => s.key == key);
    static int SavedUpgradeLv(StatType statType) => Saved<UserKSData>(UserKingSlimeManager.UserDataFileName).userKSUpgrades.FirstOrDefault(u => u.statType == statType)?.lv ?? 0;

    [UnityTest]
    public IEnumerator Lobby_InGame_Result_LevelUp_Cycle()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return CycleBody();
    }

    IEnumerator CycleBody()
    {
        UserDataManager.difficultyType = DifficultyType.Normal;
        StatType upgradeStat = UpgradeInfo.statType;

        // ── 준비: 레벨 0, 이번 판 경험치(3)로 정확히 2레벨이 오르도록 ──
        current = "준비";
        MyInfo.userMyInfoData.lv = 0;
        MyInfo.userMyInfoData.exp = MyInfo.GetMaxExp(0) + MyInfo.GetMaxExp(1) - FIRST_RUN_EXP;
        User.userData.energe = GameSetting.MAX_ENERGE;
        KSUser.GetUpgrade(upgradeStat).lv = 0;
        string stageKey = StageManager.Instance.GetStageData(DifficultyType.Normal).key;
        UserStage stage = User.userStageManager.GetUserStage(stageKey);
        stage.maxWave = 0;
        int tryCount = stage.tryCount;

        // ── 1. 로비 KS 업그레이드 (UI 버튼) ──
        yield return UpgradeInLobby(upgradeStat, 1);

        // ── 2. 전투 버튼 → 인게임: 에너지 차감, 업그레이드 반영 ──
        current = "1판 시작";
        int energe = User.userData.energe;
        yield return StartGameFromLobby();
        Assert.AreEqual(stageKey, GameManager.Instance.stageData.key, "선택한 난이도 스테이지로 시작");
        Assert.AreEqual(energe - GameSetting.PLAY_COST_ENERGE, User.userData.energe, "전투 시작 시 에너지 차감");
        AssertKingStat(upgradeStat);

        // ── 3. 사망 → 부활(광고 성공): 보상·도전 횟수 변화 없음 ──
        current = "1판 부활";
        for (int i = 0; i < RESUME_AT_WAVE; i++)
            yield return SkipWave();
        Assert.AreEqual(RESUME_AT_WAVE, GameManager.Instance.waveIdx);
        PlayerPrefs.DeleteKey(FailCanvas.DAILY_RESURRECTION_DATE_KEY); // 오늘 처음 → 5회
        int gold = User.userData.gold;
        int drawTicket = User.userData.drawTicket;
        int exp = MyInfo.userMyInfoData.exp;

        KingSlime.Instance.TakeDamage(KILL);
        yield return WaitFrames(3);
        Assert.IsTrue(FailCanvas.Instance.gameObject.activeSelf, "사망 시 실패 화면");
        FailCanvas.Instance.OnClickeResume();
        yield return WaitFrames(3);
        Assert.IsTrue(GameManager.Instance.isPlaying, "부활 후 게임 재개");
        Assert.AreEqual(tryCount, stage.tryCount, "부활은 도전 횟수에 안 셈");
        Assert.AreEqual(gold, User.userData.gold, "부활 시 골드 보상 없음");
        Assert.AreEqual(exp, MyInfo.userMyInfoData.exp, "부활 시 경험치 보상 없음");

        // ── 4. 계속 진행 → 사망 → 포기 → 결과 화면 보상 ──
        current = "1판 결과";
        while (GameManager.Instance.waveIdx < FIRST_RUN_WAVE)
            yield return SkipWave();
        Assert.AreEqual(FIRST_RUN_WAVE, GameManager.Instance.waveIdx);
        SlimeSpawner.Instance.mergeCount = FIRST_RUN_MERGE;

        KingSlime.Instance.TakeDamage(KILL);
        yield return WaitFrames(3);
        FailCanvas.Instance.OnClickedCancel();
        yield return WaitFrames(3);
        yield return WaitSaveFlushed();
        Assert.IsTrue(ResultCanvas.Instance.gameObject.activeSelf, "결과 화면");
        Assert.IsTrue(ResultCanvas.Instance.newRecordObj.activeSelf, "최고 기록 갱신 표시");

        Assert.AreEqual(gold + FIRST_RUN_WAVE, User.userData.gold, "골드 = waveIdx");
        Assert.AreEqual(drawTicket + FIRST_RUN_MERGE, User.userData.drawTicket, "뽑기권 = 합성 횟수");
        Assert.AreEqual(exp + FIRST_RUN_EXP, MyInfo.userMyInfoData.exp, "경험치 = waveIdx / 2");
        Assert.AreEqual(0, MyInfo.userMyInfoData.lv, "레벨업은 로비에서 (인게임에선 경험치만)");
        Assert.AreEqual(tryCount + 1, stage.tryCount, "부활 포함 한 판 = 도전 1회");
        Assert.AreEqual(FIRST_RUN_WAVE, stage.maxWave, "최고 wave 기록");

        Assert.AreEqual(User.userData.gold, SavedUserData.gold, "저장: 골드");
        Assert.AreEqual(User.userData.drawTicket, SavedUserData.drawTicket, "저장: 뽑기권");
        Assert.AreEqual(User.userData.energe, SavedUserData.energe, "저장: 에너지");
        Assert.AreEqual(MyInfo.userMyInfoData.exp, SavedMyInfo.exp, "저장: 경험치");
        Assert.AreEqual(stage.tryCount, SavedStage(stageKey).tryCount, "저장: 도전 횟수");
        Assert.AreEqual(FIRST_RUN_WAVE, SavedStage(stageKey).maxWave, "저장: 최고 wave");

        // ── 5. 홈 → 로비: 레벨업 창이 레벨마다 하나씩 (0 → 1 → 2), 남은 경험치 0 ──
        current = "로비 레벨업";
        ResultCanvas.Instance.OnClickedHome();
        yield return WaitScene("Lobby");
        yield return WaitUntil(() => Lobby.UserLevelUpCanvas.Instance != null && Lobby.UserLevelUpCanvas.Instance.gameObject.activeInHierarchy, "레벨업 창이 안 열림");

        for (int lv = 1; lv <= 2; lv++)
        {
            var levelUp = Lobby.UserLevelUpCanvas.Instance;
            Assert.IsTrue(levelUp.gameObject.activeInHierarchy, $"Lv.{lv} 레벨업 창");
            Assert.AreEqual(lv, MyInfo.userMyInfoData.lv, $"Lv.{lv} 레벨");
            Assert.AreEqual(lv.ToString(), levelUp.lvText.text, $"Lv.{lv} 레벨업 창 표시");
            AssertMyInfoPanels();
            levelUp.CloseCanvas();
            yield return WaitFrames(3);
        }
        Assert.IsFalse(Lobby.UserLevelUpCanvas.Instance.gameObject.activeInHierarchy, "레벨이 2만 올라야 함");
        Assert.AreEqual(2, MyInfo.userMyInfoData.lv);
        Assert.AreEqual(0, MyInfo.userMyInfoData.exp, "레벨업 후 남은 경험치");
        AssertMyInfoPanels();
        yield return WaitSaveFlushed();
        Assert.AreEqual(2, SavedMyInfo.lv, "저장: 레벨");
        Assert.AreEqual(0, SavedMyInfo.exp, "저장: 레벨업 후 경험치");
        Assert.AreEqual(1, KSUser.GetUpgrade(upgradeStat).lv, "로비 복귀 후 업그레이드 유지");

        // ── 6. 다시 업그레이드 → 2판: 바로 사망(wave 0) → 보상 0, 최고 기록 유지 ──
        yield return UpgradeInLobby(upgradeStat, 2);
        current = "2판";
        yield return StartGameFromLobby();
        AssertKingStat(upgradeStat);

        gold = User.userData.gold;
        drawTicket = User.userData.drawTicket;
        SlimeSpawner.Instance.mergeCount = 0;
        KingSlime.Instance.TakeDamage(KILL);
        yield return WaitFrames(3);
        FailCanvas.Instance.OnClickedCancel();
        yield return WaitFrames(3);
        yield return WaitSaveFlushed();
        Assert.IsFalse(ResultCanvas.Instance.newRecordObj.activeSelf, "기록보다 낮은데 신기록 표시");
        Assert.AreEqual(gold, User.userData.gold, "wave 0 사망: 골드 0");
        Assert.AreEqual(drawTicket, User.userData.drawTicket, "합성 0: 뽑기권 0");
        Assert.AreEqual(0, MyInfo.userMyInfoData.exp, "wave 0 사망: 경험치 0");
        Assert.AreEqual(tryCount + 2, stage.tryCount, "2판째 도전 횟수");
        Assert.AreEqual(FIRST_RUN_WAVE, stage.maxWave, "낮은 기록으로 최고 wave가 안 바뀜");
        Assert.AreEqual(tryCount + 2, SavedStage(stageKey).tryCount, "저장: 도전 횟수");
        Assert.AreEqual(FIRST_RUN_WAVE, SavedStage(stageKey).maxWave, "저장: 최고 wave");

        ResultCanvas.Instance.OnClickedHome();
        yield return WaitScene("Lobby");
        yield return WaitFrames(30);
        Assert.IsFalse(Lobby.UserLevelUpCanvas.Instance.gameObject.activeInHierarchy, "경험치가 모자란데 레벨업 창");
        Assert.AreEqual(2, MyInfo.userMyInfoData.lv);
        yield return WaitSaveFlushed();
        Assert.AreEqual(2, SavedUpgradeLv(upgradeStat), "저장: 업그레이드 Lv");
        AssertNoErrors();
    }

    // 저장은 UserDataManager.LateUpdate에서 몰아서 씀 → 대기 목록이 빌 때까지 기다린 뒤 파일 확인
    // (EditMode 테스트의 yield null은 에디터 틱이라 몇 번 기다려도 게임 프레임이 돈다는 보장이 없음)
    IEnumerator WaitSaveFlushed()
    {
        var field = typeof(UserDataManager).GetField("dirtySaveManagers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yield return WaitUntil(() => ((System.Collections.IList)field.GetValue(User)).Count == 0, "저장 대기 목록이 비지 않음");
    }

    // KSManagementCanvas에서 업그레이드 버튼 → Lv·재화·저장 확인
    IEnumerator UpgradeInLobby(StatType statType, int expectedLv)
    {
        current = $"업그레이드 → Lv.{expectedLv}";
        KSUpgradeInfo info = KSUpgradeData.Instance.GetUpgradeInfo(statType);
        Lobby.LobbyManager.Instance.OpenCanvas(LobbyState.KingSlimeManagement);
        yield return WaitFrames(3);
        var canvas = Object.FindFirstObjectByType<KSManagementCanvas>(FindObjectsInactive.Include);
        Assert.IsTrue(canvas != null && canvas.gameObject.activeInHierarchy, "KSManagementCanvas가 안 열림");
        KSUpgradePanel panel = canvas.GetComponentsInChildren<KSUpgradePanel>(true).FirstOrDefault(p => p.upgradeType == statType);
        Assert.IsNotNull(panel, $"{statType} 업그레이드 패널 없음");

        int price = info.GetPrice(expectedLv - 1);
        if (info.currencyType == CurrencyType.Gold) User.userData.gold += price;
        else if (info.currencyType == CurrencyType.Dia) User.userData.dia += price;
        int currency = User.userData.GetCurrency(info.currencyType);
        canvas.UpdateCanvas();

        panel.OnClickedUpgradeButton();
        yield return WaitFrames(3);
        yield return WaitSaveFlushed();
        Assert.AreEqual(expectedLv, KSUser.GetUpgrade(statType).lv, $"{statType} Lv");
        Assert.AreEqual(currency - price, User.userData.GetCurrency(info.currencyType), $"{info.currencyType} {price} 차감");
        Assert.AreEqual(expectedLv, SavedUpgradeLv(statType), "저장: 업그레이드 Lv");
        Assert.AreEqual(User.userData.GetCurrency(info.currencyType), SavedUserData.GetCurrency(info.currencyType), "저장: 업그레이드 재화");
    }

    // 로비 전투 화면의 시작 버튼 → InGame 시작까지
    IEnumerator StartGameFromLobby()
    {
        Lobby.LobbyManager.Instance.OpenCanvas(LobbyState.Battle);
        yield return WaitFrames(3);
        var battle = Lobby.LobbyManager.Instance.GetLobbyCanvas(LobbyState.Battle) as BattleCanvas;
        Assert.IsNotNull(battle, "BattleCanvas 없음");
        battle.OnClickedBtn();
        yield return WaitScene("InGame");
        yield return WaitUntil(() => GameManager.Instance != null && GameManager.Instance.isPlaying, "인게임이 시작되지 않음");
    }

    void AssertKingStat(StatType statType)
    {
        float expected = ExpectedStat(statType);
        Assert.AreEqual(expected, KingSlime.Instance.statMgr.GetValue(statType), EPSILON, $"인게임 {statType} (기본 + 업그레이드)");
        if (statType == StatType.MaxHp)
            Assert.AreEqual(expected, KingSlime.Instance.health.MaxHp, EPSILON, "킹 최대 체력");
    }

    static void AssertMyInfoPanels()
    {
        var data = MyInfo.userMyInfoData;
        foreach (var panel in Object.FindObjectsByType<MyInfoPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Assert.AreEqual($"LV.{data.lv}", panel.lvText.text, "MyInfoPanel 레벨 표시");
            Assert.AreEqual($"{data.exp}/{MyInfo.GetMaxExp()}", panel.expText.text, "MyInfoPanel 경험치 표시");
        }
    }

    IEnumerator WaitScene(string sceneName)
    {
        yield return WaitUntil(() => SceneManager.GetActiveScene().name == sceneName, $"{sceneName} 씬으로 안 넘어감");
        if (sceneName == "Lobby")
            yield return WaitUntil(() => Lobby.LobbyManager.Instance != null && Lobby.LobbyManager.Instance.lobbyState == LobbyState.Battle, "로비가 시작되지 않음");
    }

    IEnumerator WaitUntil(System.Func<bool> condition, string failMessage)
    {
        double end = EditorApplication.timeSinceStartup + SCENE_TIMEOUT;
        while (!condition())
        {
            if (EditorApplication.timeSinceStartup > end)
                Assert.Fail($"{failMessage} (씬 {SceneManager.GetActiveScene().name})\n{string.Join("\n", errors.Distinct())}");
            yield return WaitRealtime(0f);
        }
    }
}
