using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveTimerPanel : MonoBehaviour
{
    // public GameObject waveObject;
    // public Image waveTimeBar;
    public TMP_Text timeText; // 현재 wave 남은 시간 "분:초"
    // WaveTimer waveTimer;

    void Start()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);

        timeText.gameObject.SetActive(false);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {
        // (2026-11-24) 카드 소환 모드는 wave 시간 제한이 없어 남은 시간을 보여 주지 않음
        if (GameSetting.USE_WAVE_MINION_OFFER)
            return;
        timeText.gameObject.SetActive(true);
    }

    int shownSec = -1;

    void Update()
    {
        if (!GameManager.Instance.isPlaying)
            return;
        if (!timeText.gameObject.activeSelf)
            return;

        int remainSec = Mathf.CeilToInt(GameManager.Instance.mainWaveTimer);
        if (shownSec != remainSec)
        {
            shownSec = remainSec;
            timeText.text = $"{remainSec / 60}:{remainSec % 60:00}";
        }
    }
}
