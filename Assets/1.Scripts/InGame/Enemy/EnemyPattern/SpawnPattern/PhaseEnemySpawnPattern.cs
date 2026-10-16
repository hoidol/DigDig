using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

// 현재 phase의 EnemySpawnPatternData대로 반복 스폰
// 스폰 수 배율 = 1 + (peakCountMultiplier(phase) - 1) * waveCurve(웨이브 진행률) * extraCountMultiplier
// → 초반 phase는 거의 일정, 후반 phase는 웨이브 막바지에 쏟아짐
public class PhaseEnemySpawnPattern : SpawnPattern
{
    [Tooltip("웨이브 진행률(0~1) → 배율 증가 비중(0~1). 기본 t^3: 앞부분 완만, 막바지 급증")]
    public AnimationCurve waveCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 3f, 0f));
    [Tooltip("웨이브별 추가 배율 (Normal 1, Hard 1.2 등)")]
    public float extraCountMultiplier = 1f;
    [Tooltip("필드 적 수가 이 이상이면 스폰 건너뜀 (0 이하면 제한 없음)")]
    public int maxActiveEnemyCount = 150;

    WaveTimer waveTimer;
    CancellationTokenSource cts;

    void Awake()
    {
        BattleWave battleWave = GetComponentInParent<BattleWave>();
        waveTimer = battleWave != null ? battleWave.waveTimer : null;
    }

    public override void StartPattern()
    {
        EndPattern();

        PhaseData phaseData = GameManager.Instance.stageData.phaseDatas[GameManager.Instance.phase];
        if (phaseData.enemySpawnPatternDatas == null)
            return;

        // BattleWave.Awake에서 AddComponent되는 경우 대비
        if (waveTimer == null)
        {
            BattleWave battleWave = GetComponentInParent<BattleWave>();
            if (battleWave != null)
                waveTimer = battleWave.waveTimer;
        }

        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        foreach (var spawnData in phaseData.enemySpawnPatternDatas)
            SpawnLoop(spawnData, phaseData.peakCountMultiplier, cts.Token).Forget();
    }

    async UniTaskVoid SpawnLoop(EnemySpawnPatternData spawnData, float peakCountMultiplier, CancellationToken token)
    {
        if (spawnData.intervalRange.y == 0)
            return;

        while (true)
        {
            int count = GetScaledCount(spawnData.GetSpawnCount(), peakCountMultiplier);
            for (int i = 0; i < count; i++)
            {
                if (maxActiveEnemyCount > 0 && EnemySpawner.Instance.ActiveEnemyCount >= maxActiveEnemyCount)
                    break;
                Spawn(spawnData.enemyType);
            }

            float wait = Random.Range(spawnData.intervalRange.x, spawnData.intervalRange.y);
            if (await UniTask.Delay(TimeSpan.FromSeconds(wait), cancellationToken: token).SuppressCancellationThrow())
                return;
        }
    }

    public float GetCountMultiplier(float peakCountMultiplier)
    {
        float progress = waveTimer != null ? waveTimer.Progress : 0f;
        return (1f + (peakCountMultiplier - 1f) * waveCurve.Evaluate(progress)) * extraCountMultiplier;
    }

    // 소수부는 확률로 처리 (기본 1마리 * 1.3배 → 30% 확률로 2마리)
    int GetScaledCount(int baseCount, float peakCountMultiplier)
    {
        float scaled = baseCount * GetCountMultiplier(peakCountMultiplier);
        int count = Mathf.FloorToInt(scaled);
        if (Random.value < scaled - count)
            count++;
        return count;
    }

    public void Spawn(EnemyType type)
    {
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(type);
        Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        enemy?.Spawn(EnemySpawner.Instance.GetSpawnPosition());
    }

    public override void EndPattern()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
