using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BossWave : Wave
{

    Boss boss;
    bool bossDead;
    bool normalSpawnDone = true;
    CancellationTokenSource cts;

    public override void StartWave(int wIdx, DefenceType dType)
    {
        base.StartWave(wIdx, dType);
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
        boss = Instantiate(GameManager.Instance.stageData.boss);
        boss.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), wIdx); // BossSpawnEvent는 Boss.Spawn에서 발행, 체력바는 머리 위(EnemyWorldCanvas)
        // (2026-11-26) 마지막 보스 wave(50) = 보스 + 일반 적 FINAL_BOSS_WAVE_NORMAL_COUNT(50)마리, FINAL_BOSS_WAVE_SPAWN_INTERVAL(2초)마다 1마리
        // wave 클리어 = 보스 처치 + 필드 적 0 (2026-11-27 보스를 잡으면 남은 소환 취소)
        if (GameManager.IsLastWave(wIdx) && GameSetting.FINAL_BOSS_WAVE_NORMAL_COUNT > 0)
        {
            normalSpawnDone = false;
            cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            SpawnNormalLoop(GameSetting.FINAL_BOSS_WAVE_NORMAL_COUNT, cts.Token).Forget();
        }
    }

    async UniTaskVoid SpawnNormalLoop(int count, CancellationToken token)
    {
        for (int i = 0; i < count; i++)
        {
            BattleWave.SpawnEnemy(EnemyType.Normal, waveIdx, waveDefenceType);
            if (i == count - 1)
                break;
            if (await UniTask.Delay(TimeSpan.FromSeconds(GameSetting.FINAL_BOSS_WAVE_SPAWN_INTERVAL), cancellationToken: token).SuppressCancellationThrow())
                return;
        }
        normalSpawnDone = true;
        RefreshSpawnEnd();
    }

    // 보스를 잡았고 일반 적 소환도 끝나야 소환 끝
    void RefreshSpawnEnd()
    {
        if (bossDead && normalSpawnDone)
            isSpawnEnd = true;
    }

    // 보스 wave는 시간으로 끝나지 않음. 보스 처치 시 종료
    public override void EndWave()
    {
        isEnd = true;
    }

    // 보스 처치 → wave 종료 후 바로 다음 wave (마지막 wave 보스면 StartNextWave가 게임 클리어)
    void OnBossDeadEvent(BossDeadEvent e)
    {
        if (e.boss != boss)
            return;
        boss = null;
        GameEventBus.Unsubscribe<BossDeadEvent>(OnBossDeadEvent);

        if (Slime.Instance.CurHp <= 0)
            return;

        bossDead = true;
        // (2026-11-27) 보스를 잡으면 일반 적은 더 소환하지 않음 (필드에 남은 적은 그대로)
        if (!normalSpawnDone)
        {
            cts?.Cancel();
            normalSpawnDone = true;
        }
        RefreshSpawnEnd();
        // 카드 소환 모드: 남은 적까지 다 처리되면 GameManager가 준비 단계로 (여기서 다음 wave를 바로 시작하지 않음)
        if (GameSetting.USE_WAVE_MINION_OFFER)
            return;
        base.EndWave();
        GameManager.Instance.StartNextWave();
    }

    // 부활 시 Resume에서 wave가 Destroy됨 → 보스는 스포너 풀 밖이라 ClearAllEnemies로 안 지워지므로 여기서 제거 (BossDeadEvent 없이)
    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        GameEventBus.Unsubscribe<BossDeadEvent>(OnBossDeadEvent);
        if (boss != null)
            Destroy(boss.gameObject);
    }


}
