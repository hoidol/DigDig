using TMPro;
using UnityEngine;

public class MergeCoinPanel : MonoBehaviour
{
    public TMP_Text coinText;
    void Start()
    {
        GameEventBus.Subscribe<MergeCoinEvent>(OnMergeCoinEvent);
        UpdatePanel();
    }
    void OnMergeCoinEvent(MergeCoinEvent e)
    {
        UpdatePanel();
    }
    public void UpdatePanel()
    {
        coinText.text = Character.Instance.mergeCoin.ToString();
    }
}