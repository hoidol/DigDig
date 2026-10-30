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
