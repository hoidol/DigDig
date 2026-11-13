using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// 로비 킹슬라임 업그레이드(KSManagementCanvas) → 능력치 상승·표시, 로비 → 인게임 적용 (CLAUDE.md "킹슬라임 로비 업그레이드")
public class KSUpgradePlayTest : InGamePlayTestBase
{
    const int INGAME_TEST_LV = 3;
    const float EPSILON = 0.0001f;

    static UserKingSlimeManager KSUser => UserDataManager.Instance.userKingSlimeManager;
    static UserData Data => UserDataManager.Instance.userData;
    static KingSlimeData KSData => KingSlimeManager.Instance.GetKingSlimeData(Data.CharacterName);

    static IEnumerable<StatType> AllStatTypes => Enumerable.Range(0, (int)StatType.Count).Select(i => (StatType)i);

    static float Total(StatType statType) => KingSlimeManager.Instance.GetTotalStatValue(KSData, statType);

    // 기본값 + Lv × ValuePerLv (GetTotalStatValue와 따로 계산해서 비교)
    static float Expected(StatType statType)
    {
        float baseValue = KSData.GetKingSlimeStat(statType)?.value ?? 0f;
        KSUpgradeInfo info = KSUpgradeData.Instance.GetUpgradeInfo(statType);
        float upgradeValue = info == null ? 0f : info.valuePerLv * KSUser.GetUpgrade(statType).lv;
        return baseValue + upgradeValue;
    }

    static void SetCurrency(CurrencyType type, int value)
    {
        if (type == CurrencyType.Gold)
            Data.gold = value;
        else if (type == CurrencyType.Dia)
            Data.dia = value;
        else
            Assert.Fail($"업그레이드 재화 {type} 미지원 (테스트 SetCurrency에 추가)");
    }

    // StatType마다: 패널·능력치 표시 존재, 업그레이드 → Lv+1·재화 차감·스탯 +ValuePerLv·표시 갱신, 재화 부족/최대 Lv이면 막힘
    [UnityTest]
    public IEnumerator Lobby_UpgradeEachStat()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return Lobby_UpgradeEachStatBody();
    }

    IEnumerator Lobby_UpgradeEachStatBody()
    {
        Lobby.LobbyManager.Instance.OpenCanvas(LobbyState.KingSlimeManagement);
        yield return WaitFrames(3);
        var canvas = Object.FindFirstObjectByType<KSManagementCanvas>(FindObjectsInactive.Include);
        Assert.IsNotNull(canvas, "KSManagementCanvas 없음");
        Assert.IsTrue(canvas.gameObject.activeInHierarchy, "KSManagementCanvas가 안 열림");

        KSUpgradePanel[] upgradePanels = canvas.GetComponentsInChildren<KSUpgradePanel>(true);
        CurrentKSAbilityInfoPanel[] abilityPanels = canvas.kSStatStateContainer.currentKSAbilityInfoPanels;

        Assert.IsNotEmpty(KSUpgradeData.Instance.upgradeInfos, "KSUpgradeData 비어 있음 (CSV Import 확인)");
        foreach (KSUpgradeInfo info in KSUpgradeData.Instance.upgradeInfos)
        {
            StatType statType = info.statType;
            current = statType.ToString();

            KSUpgradePanel panel = upgradePanels.FirstOrDefault(p => p.upgradeType == statType);
            Assert.IsNotNull(panel, $"{statType}: 업그레이드 패널 없음 (KSUpgradeContainer 하위에 배치)");
            CurrentKSAbilityInfoPanel abilityPanel = abilityPanels.FirstOrDefault(p => p != null && p.statType == statType);
            Assert.IsNotNull(abilityPanel, $"{statType}: 능력치 표시 패널 없음 (KSStatStateContainer.currentKSAbilityInfoPanels)");

            UserKSUpgrade upgrade = KSUser.GetUpgrade(statType);
            upgrade.lv = 0;
            SetCurrency(info.currencyType, 1000000);
            canvas.UpdateCanvas();
            Assert.IsTrue(panel.gameObject.activeSelf, $"{statType}: 데이터가 있는데 패널이 숨겨짐");
            Assert.IsTrue(panel.upgradeButton.interactable, $"{statType}: 재화가 충분한데 버튼 비활성");

            // 업그레이드 1회
            float before = Total(statType);
            int currency = Data.GetCurrency(info.currencyType);
            int price = info.GetPrice(0);
            panel.OnClickedUpgradeButton();
            yield return WaitFrames(1);

            Assert.AreEqual(1, upgrade.lv, $"{statType}: Lv");
            Assert.AreEqual(currency - price, Data.GetCurrency(info.currencyType), $"{statType}: {info.currencyType} {price} 차감");
            Assert.AreEqual(before + info.valuePerLv, Total(statType), EPSILON, $"{statType}: 스탯 +{info.valuePerLv}");
            Assert.AreEqual(Expected(statType), Total(statType), EPSILON, $"{statType}: 기본 + 업그레이드");
            Assert.AreEqual(StatData.GetValueToString(statType, Total(statType)), abilityPanel.valueText.text, $"{statType}: 능력치 표시 갱신");
            StringAssert.StartsWith("Lv.1", panel.lvText.text, $"{statType}: Lv 표시");
            Assert.AreEqual(info.GetPrice(1).ToString(), panel.priceText.text, $"{statType}: 다음 가격 표시");

            // 재화 부족 → 버튼 비활성, 눌러도 안 오름
            SetCurrency(info.currencyType, info.GetPrice(1) - 1);
            canvas.UpdateCanvas();
            Assert.IsFalse(panel.upgradeButton.interactable, $"{statType}: 재화 부족인데 버튼 활성");
            panel.OnClickedUpgradeButton();
            Assert.AreEqual(1, upgrade.lv, $"{statType}: 재화 부족인데 Lv 오름");
            Assert.AreEqual(info.GetPrice(1) - 1, Data.GetCurrency(info.currencyType), $"{statType}: 재화 부족인데 차감됨");

            // 최대 Lv → MAX, 눌러도 안 오름
            if (info.maxLv > 0)
            {
                SetCurrency(info.currencyType, 1000000);
                upgrade.lv = info.maxLv;
                canvas.UpdateCanvas();
                Assert.IsFalse(panel.upgradeButton.interactable, $"{statType}: 최대 Lv인데 버튼 활성");
                Assert.AreEqual("MAX", panel.priceText.text, $"{statType}: 최대 Lv 가격 표시");
                panel.OnClickedUpgradeButton();
                Assert.AreEqual(info.maxLv, upgrade.lv, $"{statType}: 최대 Lv을 넘음");
                Assert.AreEqual(1000000, Data.GetCurrency(info.currencyType), $"{statType}: 최대 Lv인데 차감됨");
            }
        }
        AssertNoErrors();
    }

    // 로비에서 올린 업그레이드가 인게임 킹 스탯·체력·최대 슬라임 수·시작/wave 종료 코인에 그대로 들어가는지
    [UnityTest]
    public IEnumerator LobbyUpgrade_AppliedInGame()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return LobbyUpgrade_AppliedInGameBody();
    }

    IEnumerator LobbyUpgrade_AppliedInGameBody()
    {
        foreach (KSUpgradeInfo info in KSUpgradeData.Instance.upgradeInfos)
            KSUser.GetUpgrade(info.statType).lv = info.maxLv > 0 ? Mathf.Min(INGAME_TEST_LV, info.maxLv) : INGAME_TEST_LV;

        Dictionary<StatType, float> expected = AllStatTypes.ToDictionary(t => t, Expected);

        current = "로비 → 인게임";
        SceneManager.LoadScene("InGame");
        double startTimeout = UnityEditor.EditorApplication.timeSinceStartup + 60;
        while (GameManager.Instance == null || !GameManager.Instance.isPlaying)
        {
            if (UnityEditor.EditorApplication.timeSinceStartup > startTimeout)
                Assert.Fail($"로비에서 인게임 시작 안 됨 (씬 {SceneManager.GetActiveScene().name}, GameManager {(GameManager.Instance != null)}, stageData {(GameManager.Instance != null && GameManager.Instance.stageData != null)}, "
                    + $"Fade {(FadeCanvs.Instance != null ? FadeCanvs.Instance.gameObject.scene.name + "/" + FadeCanvs.Instance.gameObject.activeInHierarchy : "null")}, timeScale {Time.timeScale}, "
                    + $"KingSlime {(KingSlime.Instance != null)})\n{string.Join("\n", errors.Distinct())}");
            yield return WaitFrames(1);
        }
        // StartGame과 같은 프레임 (적 처치 코인이 섞이기 전)
        int startCoin = KingSlime.Instance.coin;

        KingSlimeStatManager statMgr = KingSlime.Instance.statMgr;
        foreach (StatType statType in AllStatTypes)
            Assert.AreEqual(expected[statType], statMgr.GetValue(statType), EPSILON, $"{statType}: 인게임 킹 스탯 (기본 + 업그레이드)");

        Assert.AreEqual(expected[StatType.MaxHp], KingSlime.Instance.health.MaxHp, EPSILON, "킹 최대 체력");
        Assert.AreEqual(expected[StatType.MaxHp], KingSlime.Instance.CurHp, EPSILON, "시작 체력 = 최대 체력");
        Assert.AreEqual(GameSetting.MINION_MIN_EXIST_COUNT + (int)expected[StatType.MaxMinionCount], MinionFieldController.Instance.minionMaxCount, "최대 슬라임 수 (기본 + MaxMinionCount, 한 번만 더함)");
        Assert.AreEqual((int)expected[StatType.StartGold], startCoin, "시작 코인 = StartGold");

        // wave 종료 → WaveClearGold
        current = "wave 종료 코인";
        int coin = KingSlime.Instance.coin;
        GameManager.Instance.CurrentWave.waveTimer = 0f;
        double end = UnityEditor.EditorApplication.timeSinceStartup + 5;
        while (!GameManager.Instance.clearWaveIdxes.Contains(0) && UnityEditor.EditorApplication.timeSinceStartup < end)
            yield return WaitRealtime(0f);
        Assert.Contains(0, GameManager.Instance.clearWaveIdxes, "wave 0이 종료되지 않음");
        Assert.GreaterOrEqual(KingSlime.Instance.coin - coin, (int)expected[StatType.WaveClearGold], "wave 종료 코인 (처치 코인이 더해질 수 있음)");

        yield return WaitRealtime(1f);
        AssertNoErrors();
    }
}
