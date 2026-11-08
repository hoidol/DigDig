// 다이아 상품 (인앱 결제)
public class DiaProductPanel : CurrencyProductPanel
{
    public override CurrencyType RewardCurrencyType => CurrencyType.Dia;
    protected override string CurrencyName => TranslateManager.GetText("Currency.Dia");
}
