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

    public GameObject waveObject;
    public Image waveTimeBar;
    void Start()
    {

        waveTimeBar.fillAmount = 0;
        waveText.text = "";
        GameEventBus.Subscribe<BreakStartEvent>(OnBreakStartEvent);
        GameEventBus.Subscribe<WaveStartEvent>(OnWavetStartEvent);
        waveObject.SetActive(false);
        waveText.text = $"다음 웨이브까지 {GetTime(0, GameSetting.BREAK_TIME)}초";
    }

    void OnBreakStartEvent(BreakStartEvent e)
    {
        waveObject.SetActive(false);
        waveText.text = $"다음 웨이브까지 {GetTime(GameManager.Instance.breakTimer, GameSetting.BREAK_TIME)}초";
    }
    void OnWavetStartEvent(WaveStartEvent e)
    {
        waveObject.SetActive(true);
        waveText.text = $"WAVE {e.phaseIdx + 1}";
    }

    void Update()
    {
        if (!GameManager.Instance.isPlaying)
        {
            return;
        }

        if (GameManager.Instance.isBreak)
        {
            waveText.text = $"다음 웨이브까지 {GetTime(GameManager.Instance.breakTimer, GameSetting.BREAK_TIME)}초";
        }
        else
        {
            waveTimeBar.fillAmount = 1f - GameManager.Instance.waveTimer / GameManager.Instance.waveTime;
        }

    }
    public string GetTime(float timer, float time)
    {
        float remainingTime = time - timer;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return $"{minutes:D2}:{seconds:D2}";
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