using UnityEngine;

public abstract class BattleWave : Wave
{
    public bool showTimer;
    public WaveTimer waveTimer;
    public PhaseEnemySpawnPattern phaseSpawnPattern;

    public virtual void Awake()
    {
        if (showTimer && waveTimer == null)
        {
            waveTimer = gameObject.AddComponent<WaveTimer>();
        }
    }

    public override void StartWave(int phase)
    {
        if (showTimer)
        {
            waveTimer.StartTimer(phase, () =>
            {
                EndWave();
            });
        }
        phaseSpawnPattern?.StartPattern();
    }

    // 타이머 종료(또는 스킵) → 스폰 중지 → 휴식으로. 필드에 남은 적은 그대로 진행
    public override void EndWave()
    {
        if (phaseSpawnPattern != null)
            phaseSpawnPattern.EndPattern();
        MoveNextWave();
    }

    public override void StopWave()
    {
        if (phaseSpawnPattern != null)
            phaseSpawnPattern.EndPattern();
        if (waveTimer != null)
            waveTimer.Stop();
    }

    // CAN_START_NEXT_PHASE_TIME 이후 ~ 종료 SKIP_WAVE_END_BEFORE초 전까지 스킵 가능
    public bool CanSkip()
    {
        if (waveTimer == null)
            return false;
        float elapsed = waveTimer.waveTime - waveTimer.waveTimer;
        return elapsed >= GameSetting.CAN_START_NEXT_PHASE_TIME
            && elapsed < waveTimer.waveTime - GameSetting.SKIP_WAVE_END_BEFORE;
    }

    public void Skip()
    {
        if (!CanSkip())
            return;
        waveTimer.Finish();
    }
}
