using TMPro;
using UnityEngine;

public class CoinPanel : MonoBehaviour
{
    public TMP_Text coinText;
    void Start()
    {
        GameEventBus.Subscribe<CoinEvent>(OnCoinEvent);
        UpdatePanel();
    }
    void OnCoinEvent(CoinEvent e)
    {
        UpdatePanel();
    }
    public void UpdatePanel()
    {
        coinText.text = Character.Instance.coin.ToString();
    }
}