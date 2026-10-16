using UnityEngine;
using UnityEngine.UI;

public class WaveTimerPanel : MonoBehaviour
{
    public GameObject waveObject;
    public Image waveTimeBar;
    WaveTimer waveTimer;

    void Start()
    {
        GameEventBus.Subscribe<WaveTimerStartEvent>(OnWaveTimerStartEvent);

        waveObject.SetActive(false);
        waveTimeBar.fillAmount = 1;
    }

    void OnWaveTimerStartEvent(WaveTimerStartEvent e)
    {
        waveObject.SetActive(true);
        waveTimer = e.waveTimer;
        waveTimeBar.fillAmount = 1;
    }

    void Update()
    {
        if (!GameManager.Instance.isPlaying)
            return;
        if (waveTimer == null)
            return;

        if (waveTimer.waveTimer <= 0)
        {
            waveObject.SetActive(false);
            return;
        }
        waveTimeBar.fillAmount = 1f - waveTimer.waveTimer / waveTimer.waveTime;
    }
}
