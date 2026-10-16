using UnityEngine;

using UnityEngine.UI;


public class WaveTimerPanel : MonoBehaviour
{

    public GameObject waveObject;
    public Image waveTimeBar;
    void Start()
    {
        waveTimeBar.fillAmount = 0;
        // GameEventBus.Subscribe<BreakStartEvent>(OnBreakStartEvent);
        // GameEventBus.Subscribe<WaveStartEvent>(OnWavetStartEvent);
        // GameEventBus.Subscribe<BossSpawnEvent>(OnBossSpawnEvent);

        GameEventBus.Subscribe<WaveTimerStartEvent>(OnWaveTimerStartEvent);

        
        waveObject.SetActive(false);
        waveTimeBar.fillAmount =1;
    }
    WaveTimer waveTimer;
    void OnWaveTimerStartEvent(WaveTimerStartEvent e)
    {
        waveObject.SetActive(true);
        waveTimer = e.waveTimer;
        waveTimeBar.fillAmount =1;

    }

    // void OnBreakStartEvent(BreakStartEvent e)
    // {
    //     waveObject.SetActive(false);
    //     waveText.text = GetBreakText();
    // }
    // void OnWavetStartEvent(WaveStartEvent e)
    // {
    //     waveObject.SetActive(true);
    //     waveText.text = $"WAVE {e.phaseIdx + 1}";
    // }
    // void OnBossSpawnEvent(BossSpawnEvent e)
    // {
    //     waveText.text = "BOSS!";
    //     waveObject.SetActive(false);
    // }

    void Update()
    {
        if (!GameManager.Instance.isPlaying)
        {
            return;
        }
        if(waveTimer == null)
            return;

        if (waveTimer.waveTimer <= 0)
        {            
            waveObject.SetActive(false);            
            return;
        }
        waveTimeBar.fillAmount = 1f - waveTimer.waveTimer / waveTimer.waveTime;
    }

    string GetBreakText()
    {
        float remainingTime = Mathf.Max(0, GameSetting.BREAK_TIME - GameManager.Instance.breakTimer);
        return $"다음 웨이브까지 {Mathf.CeilToInt(remainingTime)}초";
    }

    public string GetTime(float timer, float time)
    {
        float remainingTime = time - timer;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return $"{minutes:D2}:{seconds:D2}";
    }


}