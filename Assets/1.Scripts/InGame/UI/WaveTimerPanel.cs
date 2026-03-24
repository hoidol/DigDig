using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;
using System;


public class WaveTimerPanel : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text timeText;

    public GameObject waitingIcon;
    public GameObject normalWaveIcon;
    public GameObject hardWaveIcon;

    private void Awake()
    {
        GameEventBus.Subscribe<UndergroundStartEvent>(StartUnderground); //이벤트 구독 
        GameEventBus.Subscribe<UndergroundEndEvent>(EndUnderground); //이벤트 구독
        GameEventBus.Subscribe<WaveStartEvent>(StartWave); //이벤트 구독 - 웨이브 시작
        GameEventBus.Subscribe<WaveEndEvent>(EndWave); //이벤트 구독 - 웨이브 끝
    }
    public void StartWave(WaveStartEvent e)
    {
        waveData = e.waveData;
        waitingIcon.SetActive(false);
        normalWaveIcon.SetActive(true);

    }

    private async UniTaskVoid StartWaveTimerUniTask()
    {
        var cancellationToken = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            float remainingTime = undergroundData.waveWaitTime - GameManager.Instance.waveWaitingTimer;
            if (remainingTime < 0f) remainingTime = 0f;

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);

            timeText.text = $"{minutes:D2}:{seconds:D2}";

            await UniTask.Delay(250, cancellationToken: cancellationToken);
        }
    }

    IEnumerator StartWaveTimer()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.25f);
        while (true)
        {
            float remainingTime = undergroundData.waveWaitTime - GameManager.Instance.waveWaitingTimer;
            if (remainingTime < 0f) remainingTime = 0f;

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);

            timeText.text = $"{minutes:D2}:{seconds:D2}";

            yield return waitForSeconds;
        }

    }


    void Start()
    {
        waitingIcon.SetActive(true);
        normalWaveIcon.SetActive(false);
        hardWaveIcon.SetActive(false);
        timeText.text = "";
    }

    public WaveData waveData;
    public UndergroundData undergroundData;
    public void EndWave(WaveEndEvent e)
    {
        waitingIcon.SetActive(true);
        normalWaveIcon.SetActive(false);
        if (GameManager.Instance.isClear)
        {
            timeText.text = $"Next Wave {undergroundData.idx + 1}-{waveData.idx + 1}";
        }
        else
        {
            timeText.text = $"Go deeper";
        }

    }
    public void StartUnderground(UndergroundStartEvent e)
    {
        undergroundData = e.undergroundData;
        waitingIcon.SetActive(true);
        normalWaveIcon.SetActive(false);
        timeText.text = $"Next Wave {e.undergroundData.idx + 1}-1";

        StartWaveTimerUniTask().Forget();
    }

    public void EndUnderground(UndergroundEndEvent e)
    {

    }



}