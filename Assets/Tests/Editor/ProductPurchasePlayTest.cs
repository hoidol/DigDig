using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// ProductPurchaseButton 구매 흐름 (로비): 재화 확인·차감 / 광고 시청 / 인앱 결제 후 onPurchased, ProductPanel.Purchased 연결
public class ProductPurchasePlayTest : InGamePlayTestBase
{
    const string GOLD_KEY = "Test_GoldProduct";
    const string AD_KEY = "Test_AdProduct";
    const string INAPP_KEY = "Test_InAppProduct";
    const int PRICE = 300;

    ProductPurchaseButton button;
    int purchasedCount;

    IEnumerator Setup()
    {
        yield return WaitLobbyStart();
        ProductManager.Instance.productDatas = ProductManager.Instance.productDatas.Concat(new[]
        {
            new ProductData { key = GOLD_KEY, currencyType = CurrencyType.Gold, price = PRICE },
            new ProductData { key = AD_KEY, currencyType = CurrencyType.Ad },
            new ProductData { key = INAPP_KEY, productId = "com.test.inapp", currencyType = CurrencyType.InApp, price = 1200 },
        }).ToArray();

        var obj = new GameObject("TestPurchaseButton", typeof(RectTransform), typeof(Button));
        button = obj.AddComponent<ProductPurchaseButton>();
        purchasedCount = 0;
        button.onPurchased.AddListener(() => purchasedCount++);
    }

    static IEnumerator WaitPurchase() => WaitFrames(5); // 콜백이 SwitchToMainThread 뒤에 실행

    static int Gold
    {
        get => UserDataManager.Instance.userData.gold;
        set => UserDataManager.Instance.userData.gold = value;
    }

    [UnityTest]
    public IEnumerator CurrencyProduct_EnoughGold_SpendsAndCalls()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return Setup();
        yield return CurrencyProduct_EnoughGold_SpendsAndCallsBody();
    }

    IEnumerator CurrencyProduct_EnoughGold_SpendsAndCallsBody()
    {
        button.SetProductKey(GOLD_KEY);
        Gold = PRICE + 50;
        button.GetComponent<Button>().onClick.Invoke(); // 실제 버튼 클릭 경로
        yield return WaitPurchase();
        Assert.AreEqual(50, Gold, "가격만큼 차감");
        Assert.AreEqual(1, purchasedCount, "onPurchased 호출");
        Assert.IsFalse(BlockCanvas.Instance.gameObject.activeSelf, "구매 후 BlockCanvas가 안 닫힘");
        AssertNoErrors();
    }

    [UnityTest]
    public IEnumerator CurrencyProduct_NotEnoughGold_DoesNothing()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return Setup();
        yield return CurrencyProduct_NotEnoughGold_DoesNothingBody();
    }

    IEnumerator CurrencyProduct_NotEnoughGold_DoesNothingBody()
    {
        button.SetProductKey(GOLD_KEY);
        Gold = PRICE - 1;
        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(PRICE - 1, Gold, "모자라면 차감 안 함");
        Assert.AreEqual(0, purchasedCount, "모자란데 onPurchased 호출됨");
        Assert.IsFalse(BlockCanvas.Instance.gameObject.activeSelf);
        AssertNoErrors();
    }

    // 광고: 테스트 중엔 광고 없이 성공 처리 후 호출. 재화는 그대로
    [UnityTest]
    public IEnumerator AdProduct_AfterAd_Calls()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return Setup();
        yield return AdProduct_AfterAd_CallsBody();
    }

    IEnumerator AdProduct_AfterAd_CallsBody()
    {
        button.SetProductKey(AD_KEY);
        int gold = Gold;
        int recordsBefore = UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(AD_KEY);
        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(1, purchasedCount, "광고 후 onPurchased 호출");
        Assert.AreEqual(gold, Gold, "광고 상품인데 재화가 바뀜");
        Assert.AreEqual(recordsBefore + 1, UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(AD_KEY), "버튼 key로 구매 기록 1번");
        AssertNoErrors();
    }

    // 인앱: 테스트 중엔 결제창 없이 바로 성공 → 기록·호출. 테스트 플래그를 끄고 결제 매니저가 없으면 실패
    [UnityTest]
    public IEnumerator InAppProduct_SkippedInTest_NoStoreFails()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return Setup();
        yield return InAppProduct_SkippedInTest_NoStoreFailsBody();
    }

    IEnumerator InAppProduct_SkippedInTest_NoStoreFailsBody()
    {
        button.SetProductKey(INAPP_KEY);
        int records = UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(INAPP_KEY);
        Assert.IsTrue(ProductManager.SkipInAppPurchaseForTest, "테스트인데 인앱 결제 생략이 꺼져 있음");
        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(1, purchasedCount, "테스트 중 인앱 결제 생략 → 성공");
        Assert.AreEqual(records + 1, UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(INAPP_KEY));

        ProductManager.SkipInAppPurchaseForTest = false;
        Assume.That(InAppPurchaseManager.Instance == null, "인앱 매니저가 있으면 실제 결제가 뜨므로 건너뜀");
        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(1, purchasedCount, "결제 매니저가 없는데 성공함");
        Assert.IsFalse(BlockCanvas.Instance.gameObject.activeSelf);
        AssertNoErrors();
    }

    // 패널 하위 버튼: 구매 기록 → onPurchased → 패널 UpdatePanel
    class CountingPanel : ProductPanel
    {
        public int updateCount;
        public override void UpdatePanel() => updateCount++;
    }

    [UnityTest]
    public IEnumerator ButtonUnderPanel_RecordsAndUpdatesPanel()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return Setup();
        yield return ButtonUnderPanel_RecordsAndUpdatesPanelBody();
    }

    IEnumerator ButtonUnderPanel_RecordsAndUpdatesPanelBody()
    {
        var panel = new GameObject("TestProductPanel").AddComponent<CountingPanel>();
        button.transform.SetParent(panel.transform);
        button.key = GOLD_KEY;
        panel.OpenPanel();
        int updates = panel.updateCount;

        Gold = PRICE;
        int records = UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(GOLD_KEY);
        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(0, Gold);
        Assert.AreEqual(records + 1, UserDataManager.Instance.userPurchaseManager.GetPurchaseCount(GOLD_KEY), "구매 기록 1번");
        Assert.AreEqual(1, purchasedCount);
        Assert.AreEqual(updates + 1, panel.updateCount, "구매 후 패널 갱신");
        AssertNoErrors();
    }

    // 하루 구매 제한: 다 쓰면 버튼 비활성화, 눌러도 구매 안 함
    [UnityTest]
    public IEnumerator DailyLimit_BlocksAfterLimit()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return Setup();
        yield return DailyLimit_BlocksAfterLimitBody();
    }

    IEnumerator DailyLimit_BlocksAfterLimitBody()
    {
        button.key = AD_KEY;
        button.dailyLimit = 2;
        button.dailyRemainText = new GameObject("Remain").AddComponent<TMPro.TextMeshProUGUI>();
        button.Refresh();
        int today = button.TodayPurchaseCount;
        Assume.That(today, Is.EqualTo(0), "오늘 이미 테스트 상품 구매 기록이 있음");
        Assert.AreEqual("2/2", button.dailyRemainText.text);

        button.Purchase();
        yield return WaitPurchase();
        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(2, purchasedCount);
        Assert.AreEqual("0/2", button.dailyRemainText.text);
        Assert.IsFalse(button.GetComponent<Button>().interactable, "제한에 도달했는데 버튼이 눌림");

        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(2, purchasedCount, "제한을 넘어 구매됨");
        AssertNoErrors();
    }

    // 골드 상품: 다이아 차감 → 골드 지급, 제목은 지급 개수
    [UnityTest]
    public IEnumerator GoldProductPanel_SpendsDiaGivesGold()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return Setup();
        yield return GoldProductPanel_SpendsDiaGivesGoldBody();
    }

    IEnumerator GoldProductPanel_SpendsDiaGivesGoldBody()
    {
        const string KEY = "Test_Gold_1000";
        ProductManager.Instance.productDatas = ProductManager.Instance.productDatas
            .Append(new ProductData { key = KEY, currencyType = CurrencyType.Dia, price = 10 }).ToArray();
        var panel = new GameObject("TestGoldPanel").AddComponent<GoldProductPanel>();
        panel.amount = 1000;
        panel.titleText = new GameObject("Title").AddComponent<TMPro.TextMeshProUGUI>();
        button.transform.SetParent(panel.transform);
        button.key = KEY;
        button.onPurchased.AddListener(panel.Purchased);
        panel.OpenPanel();
        Assert.AreEqual("골드 1,000", panel.titleText.text);

        var data = UserDataManager.Instance.userData;
        data.dia = 15;
        data.gold = 0;
        button.Purchase();
        yield return WaitPurchase();
        Assert.AreEqual(5, data.dia, "다이아 10 차감");
        Assert.AreEqual(1000, data.gold, "골드 1000 지급");

        button.Purchase(); // 다이아 5 → 모자람
        yield return WaitPurchase();
        Assert.AreEqual(5, data.dia);
        Assert.AreEqual(1000, data.gold, "다이아가 모자란데 골드 지급됨");
        AssertNoErrors();
    }
}
