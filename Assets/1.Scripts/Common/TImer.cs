using System;
using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private Coroutine timerCoroutine;
    private bool isRunning;

    private DateTime startTime;
    private DateTime endTime;

    public void StartTimer(DateTime startTime, DateTime endTime, Action<string, float> notiCallback)
    {
        if (isRunning)
            return;
        if (startTime == endTime)
            return;
        this.notiCallback = notiCallback;
        this.startTime = startTime;
        this.endTime = endTime;

        isRunning = true;
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && isRunning) // 포커스를 다시 받았을 때
        {
            DateTime currentTime = DateTime.Now;
            int remainingSeconds = (int)(endTime - currentTime).TotalSeconds;

            if (remainingSeconds <= 0)
            {
                notiCallback?.Invoke(TimeManager.FormatTime(remainingSeconds), 0);
                StopTimer();
                return;
            }

            // 남은 시간을 기준으로 타이머 다시 시작
            StopTimer();
            StartTimer(currentTime, endTime, notiCallback);
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && isRunning)
        {
            // 앱이 백그라운드로 가면 현재 시간을 기록
            startTime = DateTime.Now;
        }
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        isRunning = false;
    }
    Action<string, float> notiCallback;
    private IEnumerator TimerCoroutine()
    {
        while (isRunning)
        {
            DateTime currentTime = DateTime.Now;
            int remainingSeconds = (int)(endTime - currentTime).TotalSeconds;

            if (remainingSeconds <= 0)
            {
                notiCallback?.Invoke(TimeManager.FormatTime(remainingSeconds), 0);
                StopTimer();
                yield break;
            }

            string formattedTime = TimeManager.FormatTime(remainingSeconds);
            notiCallback?.Invoke(formattedTime, remainingSeconds);

            yield return new WaitForSeconds(1f);
        }
    }
}
