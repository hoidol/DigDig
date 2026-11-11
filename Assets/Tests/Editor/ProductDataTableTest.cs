using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Json/ProductData.csv → ProductDataTable 반영 + 데이터 검증
public class ProductDataTableTest
{
    ProductDataTable table;

    [SetUp]
    public void SetUp()
    {
        table = ScriptableObject.CreateInstance<ProductDataTable>();
        table.LoadData();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(table);

    [Test]
    public void LoadsRows()
    {
        Assert.IsNotEmpty(table.productDatas, "ProductData.csv에서 읽은 상품 없음");
        var gold = table.productDatas.First(d => d.key == "Gold_1000");
        Assert.AreEqual(CurrencyType.Dia, gold.currencyType);
        Assert.AreEqual(12f, gold.price);
        var removeAd = table.productDatas.First(d => d.key == "RemoveAd");
        Assert.AreEqual("remove_ad", removeAd.productId);
        Assert.AreEqual(4.99f, removeAd.price, 0.001f);
    }

    [Test]
    public void Keys_AreUnique()
    {
        var dup = table.productDatas.GroupBy(d => d.key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.IsEmpty(dup, "같은 key 상품이 여러 개");
    }

    // 인앱 상품은 productId가 있어야 결제됨 (없으면 재화 차감으로 처리돼 구매 불가)
    [Test]
    public void InAppProducts_HaveProductId()
    {
        var missing = table.productDatas.Where(d => d.currencyType == CurrencyType.InApp && string.IsNullOrEmpty(d.productId)).Select(d => d.key).ToList();
        Assert.IsEmpty(missing, "productId 없는 인앱 상품");
    }

    // InAppProductInfo의 상품은 모두 CSV에 InApp 행이 있어야 함(가격 표시·구매 버튼용), productId는 겹치지 않음
    [Test]
    public void InAppProductInfo_MatchesCsv()
    {
        foreach (InAppProductInfo.Info info in InAppProductInfo.PRODUCTS)
        {
            var data = table.productDatas.FirstOrDefault(d => d.key == info.key);
            Assert.IsNotNull(data, $"{info.key}: ProductData.csv에 행 없음");
            Assert.AreEqual(CurrencyType.InApp, data.currencyType, $"{info.key}: currencyType이 InApp 아님");
            Assert.AreEqual(info.productId, data.productId, $"{info.key}: productId 다름 (Import ProductData 다시 실행)");
        }
        var dupIds = InAppProductInfo.PRODUCTS.GroupBy(i => i.productId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.IsEmpty(dupIds, "productId 중복");
    }

    // 재화 상품 가격은 정수 (재화는 정수로 차감)
    [Test]
    public void CurrencyPrices_AreIntegers()
    {
        var wrong = table.productDatas
            .Where(d => d.currencyType != CurrencyType.InApp && d.currencyType != CurrencyType.Ad && !Mathf.Approximately(d.price, Mathf.Round(d.price)))
            .Select(d => $"{d.key} {d.price}").ToList();
        Assert.IsEmpty(wrong, "재화 상품 가격이 소수");
    }
}
