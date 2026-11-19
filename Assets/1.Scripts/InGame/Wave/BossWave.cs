using UnityEngine;

public class BossWave : Wave
{

    Boss boss;
    public override void StartWave(int wIdx, DefenceType dType)
    {
        base.StartWave(wIdx, dType);
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
        boss = Instantiate(GameManager.Instance.stageData.boss);
        boss.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), wIdx); // BossSpawnEvent는 Boss.Spawn에서 발행, 체력바는 머리 위(EnemyWorldCanvas)
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

        if (KingSlime.Instance.CurHp <= 0)
            return;

        isSpawnEnd = true;
        // 카드 소환 모드: 남은 적까지 다 처리되면 GameManager가 준비 단계로 (여기서 다음 wave를 바로 시작하지 않음)
        if (GameSetting.USE_WAVE_MINION_OFFER)
            return;
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
