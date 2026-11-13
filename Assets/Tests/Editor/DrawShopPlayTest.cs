using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 로비 실제 뽑기 버튼(ProductData.csv 가격) → 재화 차감·지급 개수·특별 뽑기·광고 하루 제한, 슬라임 관리 화면 노출
public class DrawShopPlayTest : InGamePlayTestBase
{
    static IEnumerator WaitPurchase() => WaitFrames(5);

    static ProductPurchaseButton FindButton(string key)
    {
        var button = Object.FindObjectsByType<ProductPurchaseButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(b => b.key == key);
        Assert.IsNotNull(button, $"씬에 '{key}' 버튼 없음");
        button.Refresh(); // 상점이 닫혀 있으면 OnEnable이 안 불렸음
        Assert.IsNotNull(button.ProductData, $"'{key}' ProductData 없음");
        return button;
    }

    static DrawProductPanel PanelOf(ProductPurchaseButton button) => button.GetComponentInParent<DrawProductPanel>(true);

    static UserData Data => UserDataManager.Instance.userData;
    static UserPurchaseManager Purchases => UserDataManager.Instance.userPurchaseManager;

    static void FillCurrency()
    {
        Data.dia = 100000;
        Data.gold = 100000;
        Data.drawTicket = 100000;
    }

    static int GetCurrency(CurrencyType type) => Data.GetCurrency(type);

    // (key, 지급 개수). 권능 뽑기는 삭제(2026-11-09, 권능은 레벨 해금 선택)
    static readonly (string key, int count)[] DRAWS =
    {
        ("DrawMinion_Normal_Ad", 10),
        ("DrawMinion_Normal_DrawTicket", 20),
        ("DrawMinion_Unique_Dia", 10),
    };

    [UnityTest]
    public IEnumerator AllDrawButtons_SpendAndGrant()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return AllDrawButtons_SpendAndGrantBody();
    }

    IEnumerator AllDrawButtons_SpendAndGrantBody()
    {
        FillCurrency();
        foreach (var (key, count) in DRAWS)
        {
            current = key;
            ProductPurchaseButton button = FindButton(key);
            DrawProductPanel panel = PanelOf(button);
            Assert.IsNotNull(panel, $"{key}: 상위 뽑기 패널 없음");
            ProductData product = button.ProductData;

            int currencyBefore = product.currencyType == CurrencyType.Ad ? 0 : GetCurrency(product.currencyType);
            int records = Purchases.GetPurchaseCount(key);

            button.Purchase();
            yield return WaitPurchase();

            if (product.currencyType != CurrencyType.Ad)
                Assert.AreEqual(currencyBefore - Mathf.RoundToInt(product.price), GetCurrency(product.currencyType), $"{key}: {product.currencyType} {product.price} 차감");
            Assert.AreEqual(records + 1, Purchases.GetPurchaseCount(key), $"{key}: 구매 기록");
            Assert.IsNotNull(panel.LastDrawKeys, $"{key}: 뽑기가 실행되지 않음 (onPurchased 연결 확인)");
            Assert.AreEqual(count, panel.LastDrawKeys.Length, $"{key}: 지급 개수");

            foreach (string k in panel.LastDrawKeys)
                Assert.IsTrue(UserDataManager.Instance.userMinionManager.GetUserMinion(k).own, $"{key}: 뽑은 슬라임 {k} 미보유");
        }
        AssertNoErrors();
    }

    // 광고 뽑기 하루 3회
    [UnityTest]
    public IEnumerator AdDraw_DailyLimit3()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return AdDraw_DailyLimit3Body();
    }

    IEnumerator AdDraw_DailyLimit3Body()
    {
        foreach (string key in new[] { "DrawMinion_Normal_Ad" })
        {
            current = key;
            ProductPurchaseButton button = FindButton(key);
            Assert.AreEqual(3, button.dailyLimit, $"{key}: 하루 제한");
            int before = Purchases.GetPurchaseCount(key);
            for (int i = 0; i < 5; i++)
            {
                button.Purchase();
                yield return WaitPurchase();
            }
            Assert.AreEqual(3, Purchases.GetTodayPurchaseCount(key), $"{key}: 하루 3회까지만");
            Assert.AreEqual(0, button.DailyRemainCount);
        }
        AssertNoErrors();
    }

    // 고급 슬라임 상자: specialDrawInterval회마다 1개는 특별 확률표(S 이상)
    [UnityTest]
    public IEnumerator UniqueMinion_SpecialDrawEveryInterval()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return UniqueMinion_SpecialDrawEveryIntervalBody();
    }

    IEnumerator UniqueMinion_SpecialDrawEveryIntervalBody()
    {
        const string KEY = "DrawMinion_Unique_Dia";
        FillCurrency();
        ProductPurchaseButton button = FindButton(KEY);
        var panel = (UniqueDrawMinionProductPanel)PanelOf(button);
        int interval = panel.specialDrawInterval;
        Assert.Greater(interval, 0, "특별 뽑기 주기 0");

        // 다음 구매가 interval의 배수 회차가 되도록 기록 채움 (저장 파일은 테스트 후 복원)
        while ((Purchases.GetPurchaseCount(KEY) + 1) % interval != 0)
            Purchases.AddUserPurchase(KEY);

        button.Purchase();
        yield return WaitPurchase();
        GradeType top = MinionManager.Instance.GetMinionData(panel.LastDrawKeys[0]).grade;
        Assert.GreaterOrEqual(top, GradeType.S, $"{interval}회차인데 S 이상이 없음 (최고 {top})");

        // 진행도 표시 0/interval로 돌아감
        StringAssert.Contains($"[0/{interval}]", panel.specialDrawInfoText.text);
        AssertNoErrors();
    }

    // 슬라임 관리 화면: 모든 슬라임이 한 번씩, 보유/미보유 컨테이너, 각 컨테이너 등급 높은 순, 미보유 표시, 장착 슬롯 5개
    [UnityTest]
    public IEnumerator MinionManageCanvas_ShowsAllMinions()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return MinionManageCanvas_ShowsAllMinionsBody();
    }

    IEnumerator MinionManageCanvas_ShowsAllMinionsBody()
    {
        var canvas = Object.FindFirstObjectByType<Lobby.MinionManageCanvas>(FindObjectsInactive.Include);
        Assert.IsNotNull(canvas, "MinionManageCanvas 없음");

        // 아직 미보유인 슬라임 하나를 보유로 바꿔서 화면에 반영되는지도 확인
        var minionManager = UserDataManager.Instance.userMinionManager;
        MinionData newlyOwned = MinionManager.Instance.allMinionDatas.FirstOrDefault(d => !minionManager.GetUserMinion(d.key).own);
        if (newlyOwned != null)
            minionManager.AddUserMinion(newlyOwned.key);

        canvas.OpenCanvas(null);
        yield return WaitFrames(3);

        var shown = canvas.growth1MinionEntryPanels.Where(p => p.gameObject.activeSelf).ToList();
        var shownKeys = shown.Select(p => p.minionData.key).ToList();
        var allKeys = MinionManager.Instance.allMinionDatas.Select(d => d.key).ToList();
        Assert.AreEqual(allKeys.Count, shown.Count, $"표시된 슬라임 수 (칸 {canvas.growth1MinionEntryPanels.Length}개)");
        Assert.AreEqual(shownKeys.Count, shownKeys.Distinct().Count(), "같은 슬라임이 여러 칸");
        CollectionAssert.AreEquivalent(allKeys, shownKeys, "빠진 슬라임");

        // 보유/미보유 컨테이너 분리, 미보유 표시
        foreach (var panel in shown)
        {
            bool own = minionManager.GetUserMinion(panel.minionData.key).own;
            Assert.AreEqual(own ? canvas.ownContainer : canvas.unownContainer, panel.transform.parent, $"{panel.minionData.key} 컨테이너");
            Assert.AreEqual(!own, panel.unownPanel.activeSelf, $"{panel.minionData.key} 미보유 표시");

            // 카드 틀 색 = 등급
            GradeCardColor cardColor = Grade.GetCardColor(panel.minionData.grade);
            Assert.IsNotNull(panel.minionPanel.bgImage, $"{panel.minionData.key} bgImage 연결");
            Assert.IsNotNull(panel.minionPanel.innerBorder, $"{panel.minionData.key} innerBorder 연결");
            Assert.IsNotNull(panel.minionPanel.hightLightImage, $"{panel.minionData.key} hightLightImage 연결");
            Assert.AreEqual((Color32)cardColor.bg, (Color32)panel.minionPanel.bgImage.color, $"{panel.minionData.key} 카드 Bg 색");
            Assert.AreEqual((Color32)cardColor.innerBorder, (Color32)panel.minionPanel.innerBorder.color, $"{panel.minionData.key} 카드 InnerBorder 색");
            Assert.AreEqual((Color32)cardColor.highLight, (Color32)panel.minionPanel.hightLightImage.color, $"{panel.minionData.key} 카드 HighLight 색");
        }
        Assert.Greater(shown.Select(p => (Color32)p.minionPanel.bgImage.color).Distinct().Count(), 1, "등급이 달라도 카드 색이 모두 같음");
        {
        }

        // 각 컨테이너 안에서 화면 순서 = 등급 높은 순 (레전드 → 유니크 → 레어 → 노말). 새로 얻은 슬라임도 등급 자리에
        foreach (Transform container in new[] { canvas.ownContainer, canvas.unownContainer })
        {
            var grades = container.GetComponentsInChildren<Lobby.Growth1MinionEntryPanel>(false)
                .Where(p => p.transform.parent == container && p.minionData != null)
                .OrderBy(p => p.transform.GetSiblingIndex())
                .Select(p => p.minionData.grade).ToList();
            for (int i = 1; i < grades.Count; i++)
                Assert.GreaterOrEqual((int)grades[i - 1], (int)grades[i], $"{container.name} {i}번째 칸 등급 순서: {string.Join(",", grades)}");
        }
        if (newlyOwned != null)
            Assert.IsFalse(shown.First(p => p.minionData.key == newlyOwned.key).unownPanel.activeSelf, "새로 얻은 슬라임이 미보유로 보임");

        Assert.AreEqual(minionManager.userMinionData.equiptedMinions.Length, canvas.equipedSlotPanels.Length, "장착 슬롯 수");
        AssertNoErrors();
    }
}
