using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;
using System;


public class WaveTimerPanel : MonoBehaviour, IGameListener
{
    public TMP_Text titleText;
    public TMP_Text timeText;

    public GameObject waitingIcon;
    public GameObject normalWaveIcon;
    public GameObject hardWaveIcon;

    private void Awake()
    {
        GameManager.Instance.AddGameListener(this);
    }
    void Start()
    {

        waitingIcon.SetActive(true);
        normalWaveIcon.SetActive(false);
        hardWaveIcon.SetActive(false);
        timeText.text = "";
    }

    async UniTask Test()
    {
        Debug.Log("WaveTimerPanel Test() 1");
        await UniTask.Delay(TimeSpan.FromSeconds(5.0f));
        Debug.Log("WaveTimerPanel Test() 2");
    }
    public WaveData waveData;
    public UndergroundData undergroundData;
    public void StartUnderground(UndergroundData uData)
    {
        undergroundData = uData;
        waitingIcon.SetActive(true);
        normalWaveIcon.SetActive(false);
        timeText.text = $"Next Wave {uData.idx + 1}-1";

        StartWaveTimerUniTask().Forget();
    }

    public void EndUnderground()
    {

    }

    public void StartWave(WaveData wData)
    {
        waveData = wData;
        waitingIcon.SetActive(false);
        normalWaveIcon.SetActive(true);
    }

    public void EndWave()
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

}