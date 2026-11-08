// 골드 상품 (다이아로 구매)
public class GoldProductPanel : CurrencyProductPanel
{
    public override CurrencyType RewardCurrencyType => CurrencyType.Gold;
    protected override string CurrencyName => TranslateManager.GetText("Currency.Gold");
}
