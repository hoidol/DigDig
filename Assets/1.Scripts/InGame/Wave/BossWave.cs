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

    // 보스 wave는 시간으로 끝나지 않음. 보스 처치 시 스테이지 클리어
    public override void EndWave()
    {
        isEnd = true;
    }

    void OnBossDeadEvent(BossDeadEvent e)
    {
        if (e.boss != boss)
            return;
        boss = null;

        if (Character.Instance.CurHp <= 0)
            return;
        GameManager.Instance.EndGame(true);
    }

    // 부활 시 Resume에서 wave가 Destroy됨 → 보스는 스포너 풀 밖이라 ClearAllEnemies로 안 지워지므로 여기서 제거 (BossDeadEvent 없이)
    void OnDestroy()
    {
        GameEventBus.Unsubscribe<BossDeadEvent>(OnBossDeadEvent);
        if (boss != null)
            Destroy(boss.gameObject);
    }

}
