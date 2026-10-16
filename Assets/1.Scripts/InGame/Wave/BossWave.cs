using UnityEngine;

public class BossWave : Wave
{
    void Start()
    {
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
    }

    Boss boss;

    public override void StartWave(int phase)
    {
        boss = Instantiate(GameManager.Instance.stageData.boss);
        boss.Spawn(EnemySpawner.Instance.GetSpawnPosition());
        BossCanvas.Instance.SetBoss(boss); // BossSpawnEvent는 Boss.Spawn에서 발행
    }

    // 부활 시: 보스를 BossDeadEvent 없이 제거 (StartWave에서 새로 생성)
    public override void StopWave()
    {
        if (boss != null)
            Destroy(boss.gameObject);
        boss = null;
    }

    void OnBossDeadEvent(BossDeadEvent e)
    {
        if (GameManager.Instance.currentWave != this)
            return;
        EndWave();
    }

    public override void EndWave()
    {
        base.EndWave();


        if (Character.Instance.CurHp <= 0)
            return;

        GameManager.Instance.EndGame(true);
    }
}
