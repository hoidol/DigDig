using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.UI;


public class WaveTimerPanel : MonoBehaviour
{
    public TMP_Text waveText;
    public Image dayTimeBar;
    public Image nightTimeBar;


    void Start()
    {
        dayTimeBar.fillAmount = 0;
        nightTimeBar.fillAmount = 0;
        waveText.text = "";
        GameEventBus.Subscribe<DayStartEvent>(OnDayStartEvent);
    }

    void OnDayStartEvent(DayStartEvent e)
    {
        waveText.text = $"웨이브 {e.phaseIdx + 1}";
    }

    void Update()
    {
        if (!GameManager.Instance.isPlaying)
        {
            return;
        }
        if (GameManager.Instance.isDay)
        {
            dayTimeBar.fillAmount = GameManager.Instance.dayTimer / GameSetting.DAY_TIME;
        }
        else
        {
            nightTimeBar.fillAmount = 1f - GameManager.Instance.nightTimer / GameSetting.NIGHT_TIME;
        }
        
    }
    // public void StartWave(WaveStartEvent e)
    // {
    //     waveData = e.waveData;
    //     waitingIcon.SetActive(false);
    //     normalWaveIcon.SetActive(true);

    // }

    // float waveTime;
    // public void StartUnderground(UndergroundStartEvent e)
    // {
    //     undergroundData = e.undergroundData;
    //     waveTime = StageData.WAVE_TIMES[undergroundData.idx];
    //     waitingIcon.SetActive(true);
    //     normalWaveIcon.SetActive(false);
    //     timeText.text = $"Next Wave";

    //     StartWaveTimerUniTask().Forget();
    // }

    // private async UniTaskVoid StartWaveTimerUniTask()
    // {
    //     var cancellationToken = this.GetCancellationTokenOnDestroy();
    //     while (true)
    //     {
    //         float remainingTime = waveTime - GameManager.Instance.waveWaitingTimer;
    //         if (remainingTime < 0f) remainingTime = 0f;

    //         int minutes = Mathf.FloorToInt(remainingTime / 60f);
    //         int seconds = Mathf.FloorToInt(remainingTime % 60f);

    //         timeText.text = $"{minutes:D2}:{seconds:D2}";

    //         await UniTask.Delay(250, cancellationToken: cancellationToken);
    //     }
    // }

    // public WaveData waveData;
    // public UndergroundData undergroundData;
    // public void EndWave(WaveEndEvent e)
    // {
    //     waitingIcon.SetActive(true);
    //     normalWaveIcon.SetActive(false);
    //     if (GameManager.Instance.isClear)
    //     {
    //         timeText.text = $"Next Wave {undergroundData.idx + 1}-{waveData.idx + 1}";
    //     }
    //     else
    //     {
    //         timeText.text = $"Go deeper";
    //     }

    // }

    // public void EndUnderground(UndergroundEndEvent e)
    // {

    // }



}