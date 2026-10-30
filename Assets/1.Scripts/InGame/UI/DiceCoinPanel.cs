using TMPro;
using UnityEngine;

public class DiceCoinPanel : MonoBehaviour
{
    public TMP_Text coinText;
    void Start()
    {
        GameEventBus.Subscribe<DiceCoinEvent>(OnDiceCoinEvent);
        UpdatePanel();
    }
    void OnDiceCoinEvent(DiceCoinEvent e)
    {
        UpdatePanel();
    }
    public void UpdatePanel()
    {
        coinText.text = Character.Instance.diceCoin.ToString();
    }
}