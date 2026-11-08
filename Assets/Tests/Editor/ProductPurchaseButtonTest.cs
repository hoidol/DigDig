using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ProductPurchaseButton: key로 ProductData를 찾아 가격 UI 설정
public class ProductPurchaseButtonTest
{
    GameObject managerObj;
    GameObject buttonObj;
    ProductPurchaseButton button;

    [SetUp]
    public void SetUp()
    {
        managerObj = new GameObject("ProductManager");
        var manager = managerObj.AddComponent<ProductManager>();
        manager.productDatas = new[]
        {
            new ProductData { key = "GoldProduct", currencyType = CurrencyType.Gold, price = 300 },
            new ProductData { key = "AdProduct", currencyType = CurrencyType.Ad },
            new ProductData { key = "InAppProduct", productId = "com.test.inapp", currencyType = CurrencyType.InApp, price = 1.99f },
        };
        ProductManager.Instance = manager;

        buttonObj = new GameObject("Button");
        button = buttonObj.AddComponent<ProductPurchaseButton>();
        button.inAppPurchasePriceText = new GameObject("InAppPrice").AddComponent<TextMeshProUGUI>();
        button.inAppPurchasePriceText.transform.SetParent(buttonObj.transform);
        var panelObj = new GameObject("CurrencyPanel");
        panelObj.transform.SetParent(buttonObj.transform);
        button.currencyDisplayPanel = panelObj.AddComponent<CurrencyDisplayPanel>();
        button.currencyDisplayPanel.currencyImage = new GameObject("Icon").AddComponent<Image>();
        button.currencyDisplayPanel.valueText = new GameObject("Value").AddComponent<TextMeshProUGUI>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(button.currencyDisplayPanel.currencyImage.gameObject);
        Object.DestroyImmediate(button.currencyDisplayPanel.valueText.gameObject);
        Object.DestroyImmediate(buttonObj);
        Object.DestroyImmediate(managerObj);
        ProductManager.Instance = null;
    }

    [Test]
    public void CurrencyProduct_ShowsIconAndPrice()
    {
        button.SetProductKey("GoldProduct");
        Assert.IsTrue(button.currencyDisplayPanel.gameObject.activeSelf);
        Assert.IsFalse(button.inAppPurchasePriceText.gameObject.activeSelf);
        Assert.AreEqual("300", button.currencyDisplayPanel.valueText.text);
        Assert.AreEqual(CurrencyInfo.GetSprite(CurrencyType.Gold), button.currencyDisplayPanel.currencyImage.sprite);
    }

    [Test]
    public void AdProduct_ShowsAdIcon()
    {
        button.SetProductKey("AdProduct");
        Assert.IsTrue(button.currencyDisplayPanel.gameObject.activeSelf);
        Assert.AreEqual("광고", button.currencyDisplayPanel.valueText.text);
        Assert.AreEqual(CurrencyInfo.GetSprite(CurrencyType.Ad), button.currencyDisplayPanel.currencyImage.sprite);
    }

    // 인앱 매니저가 없으면(스토어 가격 모름) price를 달러로
    [Test]
    public void InAppProduct_ShowsPriceText()
    {
        button.SetProductKey("InAppProduct");
        Assert.IsTrue(button.inAppPurchasePriceText.gameObject.activeSelf);
        Assert.IsFalse(button.currencyDisplayPanel.gameObject.activeSelf);
        Assert.AreEqual("$1.99", button.inAppPurchasePriceText.text);
    }

    [Test]
    public void UnknownKey_KeepsUiAndWarns()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("ProductData 없음 : 'Nope'"));
        button.SetProductKey("Nope");
        Assert.AreEqual("Nope", button.key);
    }
}
