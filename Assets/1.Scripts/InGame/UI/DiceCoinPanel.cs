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
        // (2026-11-22) 카드 소환 모드 = "남은/기본 지급량" (5/5, 보너스 엘리트로 더 받으면 6/5)
        KingSlime king = KingSlime.Instance;
        coinText.text = GameSetting.USE_WAVE_MINION_OFFER ? $"{king.diceCoin}/{GameSetting.WAVE_DICE_COIN}" : king.diceCoin.ToString();
    }
}