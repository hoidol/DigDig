using System;
using UnityEngine;

public class WaveTimer : MonoBehaviour
{
    public float waveTimer = 0f;
    public float waveTime = 0f;
    Action endCallback;
    // 웨이브 진행률 0~1
    public float Progress => waveTime > 0 ? Mathf.Clamp01(1f - waveTimer / waveTime) : 0f;
    bool working;
    public void StartTimer(int phase, Action endCb)
    {
        working= true;
        endCallback = endCb;

        waveTime = GameSetting.WAVE_TIME;
        waveTimer = waveTime;
        GameEventBus.Publish(new WaveTimerStartEvent(this));
    }
    void Update()
    {
        if(!working)
            return;
        
        if (waveTimer <= 0)
        {
            working= false;
            endCallback?.Invoke();
            endCallback = null;
            return;
        }
        
        waveTimer -= Time.deltaTime;
    }

    // 남은 시간을 0으로 → 다음 Update에서 종료 콜백 호출 (스킵용)
    public void Finish()
    {
        if (working)
            waveTimer = 0;
    }

    // 종료 콜백 없이 타이머 중지 (부활로 웨이브를 다시 시작할 때)
    public void Stop()
    {
        working = false;
        endCallback = null;
    }
}

public class WaveTimerStartEvent
{
    public WaveTimer waveTimer;
    public WaveTimerStartEvent(WaveTimer timer)
    {
        waveTimer = timer;
    }
}