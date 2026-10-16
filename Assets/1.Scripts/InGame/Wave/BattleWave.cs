using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BattleWave : Wave
{
    public bool showTimer;
    public WaveTimer waveTimer;
    public PhaseEnemySpawnPattern phaseSpawnPattern;    
    public SpawnPattern[] specialEnemySpawners;
    public float startSpecialEnemySpawnerTime;    // 웨이브 시작 후 첫 특수 스폰까지(초)
    public float intervalSpecialEnemySpawnerTime; // 이후 반복 간격(초), 0 이하면 1회만

    CancellationTokenSource specialSpawnCts;

    public virtual void Awake()
    {
        if (showTimer && waveTimer == null)
        {
            waveTimer = gameObject.AddComponent<WaveTimer>();
        }
        // phaseSpawnPattern = GetComponentInChildren<PhaseEnemySpawnPattern>();
        // 일반 스폰(phaseSpawnPattern)은 특수 스폰 랜덤 대상에서 제외
        specialEnemySpawners = System.Array.FindAll(GetComponentsInChildren<SpawnPattern>(), p => p != phaseSpawnPattern);
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

        // WaveData의 SpecialPattern이 TRUE인 phase에서만 특수 스폰
        if (GameManager.Instance.stageData.phaseDatas[phase].specialPattern)
            StartSpecialSpawn();
        else
            StopSpecialSpawn();

        WaitClearAndMoveNext().Forget();
    }

    public override void EndWave()
    {
        if (phaseSpawnPattern != null)
            phaseSpawnPattern.EndPattern();
        StopSpecialSpawn();
    }

    // 스폰 종료 후 필드의 모든 적이 제거되면 다음 웨이브로
    async UniTaskVoid WaitClearAndMoveNext()
    {
        bool canceled = await UniTask.WaitUntil(
            () => GameManager.Instance.isPlaying && EnemySpawner.Instance.ActiveEnemyCount <= 0,
            cancellationToken: destroyCancellationToken).SuppressCancellationThrow();
        if (canceled)
            return;

        MoveNextWave();
    }

    void StartSpecialSpawn()
    {
        StopSpecialSpawn();
        if (specialEnemySpawners == null || specialEnemySpawners.Length == 0)
            return;

        specialSpawnCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        SpecialSpawnLoop(specialSpawnCts.Token).Forget();
    }

    void StopSpecialSpawn()
    {
        if (specialSpawnCts != null)
        {
            specialSpawnCts.Cancel();
            specialSpawnCts.Dispose();
            specialSpawnCts = null;
        }

        if (specialEnemySpawners == null)
            return;
        // 경고 후 대기 중인 스폰이 다음 웨이브/휴식에 튀어나오지 않도록 정리
        for (int i = 0; i < specialEnemySpawners.Length; i++)
        {
            specialEnemySpawners[i].EndPattern();
        }
    }

    // startSpecialEnemySpawnerTime 후 첫 스폰, 이후 intervalSpecialEnemySpawnerTime 마다 스포너를 순서대로 하나씩 호출
    async UniTaskVoid SpecialSpawnLoop(CancellationToken token)
    {
        float timer = 0f;
        float nextSpawnTime = startSpecialEnemySpawnerTime;
        // int spawnerIdx = 0;

        while (true)
        {
            await UniTask.Yield();
            if (token.IsCancellationRequested)
                return;

            if (!GameManager.Instance.isPlaying)
                continue;

            timer += Time.deltaTime;
            if (timer < nextSpawnTime)
                continue;

            specialEnemySpawners[Random.Range(0,specialEnemySpawners.Length)].StartPattern();
            // spawnerIdx = (spawnerIdx + 1) % specialEnemySpawners.Length;

            if (intervalSpecialEnemySpawnerTime <= 0)
                return;
            nextSpawnTime += intervalSpecialEnemySpawnerTime;
        }
    }
}
