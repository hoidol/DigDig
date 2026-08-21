
using TMPro;
using UnityEngine;

public class ItemShopButton : ButtonUI
{
    public TMP_Text coinText;
    void Start()
    {
        GameEventBus.Subscribe< CoinEvent>(OnCoinEvent);
        coinText.text ="0";
    }
    void OnCoinEvent(CoinEvent cE)
    {
        coinText.text = cE.curCoin.ToString();
    }
    public override void OnClickedBtn()
    {
        ItemShopCanvas.Instance.OpenCanvas();
    }
}