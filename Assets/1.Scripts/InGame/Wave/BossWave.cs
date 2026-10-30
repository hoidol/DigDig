using UnityEngine;

public class BossWave : Wave
{

    Boss boss;
    public override void StartWave(int wIdx, DefenceType dType)
    {
        base.StartWave(wIdx, dType);
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
        boss = Instantiate(GameManager.Instance.stageData.boss);
        boss.Spawn(EnemySpawner.Instance.GetSpawnPosition(), wIdx);
        BossCanvas.Instance.SetBoss(boss); // BossSpawnEvent는 Boss.Spawn에서 발행
    }

    // 보스 wave는 시간으로 끝나지 않음. 보스 처치 시 종료
    public override void EndWave()
    {
        isEnd = true;
    }

    // 보스 처치 → wave 종료 후 바로 다음 wave (무한 웨이브)
    void OnBossDeadEvent(BossDeadEvent e)
    {
        if (e.boss != boss)
            return;
        boss = null;
        GameEventBus.Unsubscribe<BossDeadEvent>(OnBossDeadEvent);

        if (Character.Instance.CurHp <= 0)
            return;

        isSpawnEnd = true;
        base.EndWave();
        GameManager.Instance.StartNextWave();
    }

    // 부활 시 Resume에서 wave가 Destroy됨 → 보스는 스포너 풀 밖이라 ClearAllEnemies로 안 지워지므로 여기서 제거 (BossDeadEvent 없이)
    void OnDestroy()
    {
        GameEventBus.Unsubscribe<BossDeadEvent>(OnBossDeadEvent);
        if (boss != null)
            Destroy(boss.gameObject);
    }

}
