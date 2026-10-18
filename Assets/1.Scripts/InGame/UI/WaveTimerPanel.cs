using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveTimerPanel : MonoBehaviour
{
    public GameObject waveObject;
    public Image waveTimeBar;
    public TMP_Text breakText; // 휴식 중 "다음 웨이브까지 N초"
    // WaveTimer waveTimer;

    void Start()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);

        waveObject.SetActive(false);
        waveTimeBar.fillAmount = 0;
        breakText.gameObject.SetActive(false);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {
        waveObject.SetActive(true);
        waveTimeBar.fillAmount = 1;
    }

    int breakSec = -1;

    void Update()
    {
        if (!GameManager.Instance.isPlaying)
            return;

        // 휴식 중: TimeBar 숨기고 카운트다운 표시
        bool isBreak = GameManager.Instance.isBreak;
        if (waveObject.gameObject.activeSelf == isBreak)
            waveObject.gameObject.SetActive(!isBreak);
        if (breakText.gameObject.activeSelf != isBreak)
            breakText.gameObject.SetActive(isBreak);

        if (isBreak)
        {
            int sec = Mathf.CeilToInt(GameManager.Instance.breakTimer);
            if (breakSec != sec)
            {
                breakSec = sec;
                breakText.text = $"다음 웨이브까지 {sec}초";
            }
            return;
        }

        waveTimeBar.fillAmount = GameManager.Instance.mainWaveTimer / GameSetting.WAVE_TIME;
    }
}
