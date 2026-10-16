using UnityEngine;

public class BossWave : Wave
{
    void Start()
    {
        GameEventBus.Subscribe<BossDeadEvent>(OnBossDeadEvent);
    }

    public override void StartWave(int phase)
    {

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
