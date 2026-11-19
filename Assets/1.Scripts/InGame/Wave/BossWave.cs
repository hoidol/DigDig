using UnityEngine;

public class BossWave : Wave
{

    Boss boss;
    public override void StartWave(int wIdx, DefenceType dType)
    {
        base.StartWave(wIdx, dType);
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
        boss = Instantiate(GameManager.Instance.stageData.boss);
        boss.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), wIdx);
        BossCanvas.Instance.SetBoss(boss); // BossSpawnEvent는 Boss.Spawn에서 발행
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
        CloseBossCanvas();

        if (KingSlime.Instance.CurHp <= 0)
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
        {
            Destroy(boss.gameObject);
            CloseBossCanvas();
        }
    }

    // 보스 체력 창 닫기 (보스 처치 / 부활로 보스 제거)
    static void CloseBossCanvas()
    {
        if (BossCanvas.Instance != null && BossCanvas.Instance.gameObject.activeSelf)
            BossCanvas.Instance.CloseCanvas();
    }

}
