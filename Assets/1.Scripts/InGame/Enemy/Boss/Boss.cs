using UnityEngine;
using System;


public abstract class Boss : Enemy
{
    public BossState bossState;
    protected int currentPhase;
    public int CurrentPhase => currentPhase;

    [SerializeField] BossPhase[] phases;
    BossPhase curBossPhase;


    public override void Spawn(Vector2Int[,] idxArr)
    {
        for (int i = 0; i < phases.Length; i++)
        {
            phases[i].Init(this);
        }
        bossState = BossState.IntroAnim;
        currentPhase = 0;
        base.Spawn(idxArr);
        OnEnterPhase(0);
        GameEventBus.Publish(new BossSpawnEvent(this));
    }

    public override void Update()
    {
        if (bossState == BossState.IntroAnim)
            return;

        base.Update();
    }
    public override void UpdateAttack()
    {

    }
    protected override void OnHpChanged()
    {
        base.OnHpChanged();
        CheckPhaseTransition();
    }

    public override void OnDead()
    {
        base.OnDead();
        GameEventBus.Publish(new BossDeadEvent(this));
    }

    void CheckPhaseTransition()
    {
        float hpRate = curHp / maxHp;
        int newPhase = 0;
        for (int i = 0; i < phases.Length; i++)
        {
            if (hpRate <= phases[i].phaseThreshold)
                newPhase = i + 1;
        }

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            GameEventBus.Publish(new BossPhaseChangedEvent(this, currentPhase));
            OnEnterPhase(currentPhase);
        }
    }
    protected virtual void OnEnterPhase(int phase)
    {
        curBossPhase = phases[phase];
        curBossPhase.StartPhase();
        ChangeState(EnemyState.Waiting);
    }
    public virtual float PlayAnim(string anim)
    {
        return 0;
    }


    public override void CancelAttack()
    {
        if (phases != null && currentPhase < phases.Length)
            phases[currentPhase].EndPhase();

        EndAttack();
    }

}

public enum BossState
{
    IntroAnim,//등장 연출
    Normal
}