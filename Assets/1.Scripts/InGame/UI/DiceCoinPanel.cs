using TMPro;
using UnityEngine;

public class DiceCoinPanel : MonoBehaviour
{
    public TMP_Text coinText;
    void Start()
    {
        GameEventBus.Subscribe<DiceCoinEvent>(OnDiceCoinEvent);
        GameEventBus.Subscribe<LevelUpCanvasClosedEvent>(OnLevelUpClosed); // (2026-11-25) 레벨업 창을 닫으면 갱신
        UpdatePanel();
    }
    void OnDestroy()
    {
        GameEventBus.Unsubscribe<DiceCoinEvent>(OnDiceCoinEvent);
        GameEventBus.Unsubscribe<LevelUpCanvasClosedEvent>(OnLevelUpClosed);
    }
    // 꺼져 있던 동안(wave 중·레벨업 창) 바뀐 값도 다시 켜질 때 반영
    void OnEnable()
    {
        if (KingSlime.Instance != null)
            UpdatePanel();
    }
    void OnDiceCoinEvent(DiceCoinEvent e)
    {
        UpdatePanel();
    }
    void OnLevelUpClosed(LevelUpCanvasClosedEvent e)
    {
        UpdatePanel();
    }
    public void UpdatePanel()
    {
        // (2026-11-22) 카드 소환 모드 = "남은/기본 지급량" (5/5, 보너스 엘리트로 더 받으면 6/5)
        KingSlime king = KingSlime.Instance;
        if (king == null || coinText == null)
            return;
        coinText.text = GameSetting.USE_WAVE_MINION_OFFER ? $"{king.diceCoin}/{GameSetting.WAVE_DICE_COIN}" : king.diceCoin.ToString();
    }
}
