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

        waveTime = GameSetting.WAVE_TIME + GameSetting.WAVE_INCREASE_TIME * phase;

        
        if (waveTime >= GameSetting.MIX_WAVE_TIME)
        {
            waveTime = GameSetting.MIX_WAVE_TIME;
        }
        waveTimer = waveTime;
        GameEventBus.Publish(new WaveTimerStartEvent(phase,this));
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
}

public class WaveTimerStartEvent
{
    public int phase;
    public WaveTimer waveTimer;
    public WaveTimerStartEvent(int phase, WaveTimer timer)
    {
        waveTimer = timer;
    }
}