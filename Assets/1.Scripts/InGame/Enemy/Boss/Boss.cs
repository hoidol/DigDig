using UnityEngine;
using System;


public abstract class Boss : Enemy
{
    public BossState bossState;
    protected int currentPhase;
    public int CurrentPhase => currentPhase;

    [SerializeField] BossPhase[] phases;
    BossPhase curBossPhase;
    DamageData damageData = new DamageData();

    IBossMovement movement;


    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<IBossMovement>();
    }

    public override void Init(EnemyData data)
    {
        base.Init(data);
        //bossData = data as BossData;
    }

    public override void Spawn(Vector2Int[,] idxArr)
    {
        bossState = BossState.IntroAnim;
        currentPhase = 0;
        base.Spawn(idxArr);
        OnEnterPhase(0);
        damageData.damage = enemyData.GetAttackPower();
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
        movement?.Cancel();
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
            ChangeState(EnemyState.PhaseTransition);
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

    protected override void StartAttack()
    {
        base.StartAttack();

    }

    // 공격 완료 후 → Dash → EndAttack 순서
    void AfterAttack()
    {
        if (movement == null) { EndAttack(); return; }

        ChangeState(EnemyState.Dash);
        movement.StartMove(this, EndAttack);
    }

    public override void CancelAttack()
    {
        if (phases != null && currentPhase < phases.Length)
            phases[currentPhase].EndPhase();

        movement?.Cancel();
        EndAttack();
    }
    // Dash 중 플레이어와 접촉 시 데미지
    void OnTriggerEnter2D(Collider2D other)
    {
        if (state != EnemyState.Dash) return;
        if (other.CompareTag("Player"))
            Player.Instance.TakeDamage(damageData);
    }

}

// [Serializable]
// public class BossPhase
// {
//     public BossAttackPattern[] patterns;

//     int patternIndex;

//     public BossAttackPattern GetNextPattern()
//     {
//         if (patterns == null || patterns.Length == 0) return null;

//         BossAttackPattern pattern = patterns[patternIndex];
//         patternIndex = (patternIndex + 1) % patterns.Length;
//         return pattern;
//     }

//     public void CancelCurrent()
//     {
//         if (patterns == null || patterns.Length == 0) return;

//         int cur = (patternIndex - 1 + patterns.Length) % patterns.Length;
//         patterns[cur]?.Cancel();
//     }

//     public void Reset() => patternIndex = 0;
// }
public enum BossState
{
    IntroAnim//등장 연출
}