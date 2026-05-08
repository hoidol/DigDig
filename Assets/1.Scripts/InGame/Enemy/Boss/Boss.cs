using UnityEngine;
using System;

public abstract class Boss : Enemy
{
    protected BossData bossData;
    protected int currentPhase;
    public int CurrentPhase => currentPhase;

    [SerializeField] BossPhase[] phases;
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
        bossData = data as BossData;
    }

    public override void Spawn(Vector2 pos)
    {
        currentPhase = 0;
        base.Spawn(pos);
        OnEnterPhase(0);
        damageData.damage = enemyData.GetAttackPower();
        GameEventBus.Publish(new BossSpawnEvent(this));
    }

    protected override void OnHpChanged(float cur, float max)
    {
        base.OnHpChanged(cur, max);
        CheckPhaseTransition(cur / max);
    }

    protected override void OnDead(DamageData damageData)
    {
        movement?.Cancel();
        base.OnDead(damageData);
        GameEventBus.Publish(new BossDeadEvent(this));
    }

    void CheckPhaseTransition(float hpRate)
    {
        if (bossData?.phaseThresholds == null) return;

        int newPhase = 0;
        for (int i = 0; i < bossData.phaseThresholds.Length; i++)
        {
            if (hpRate <= bossData.phaseThresholds[i])
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
        ChangeState(EnemyState.Approaching);
    }

    protected override void StartAttack()
    {
        base.StartAttack();

        BossAttackPattern pattern = GetCurrentPattern();
        if (pattern == null) { AfterAttack(); return; }

        pattern.Execute(this, AfterAttack);
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
            phases[currentPhase].CancelCurrent();

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

    BossAttackPattern GetCurrentPattern()
    {
        if (phases == null || currentPhase >= phases.Length) return null;
        return phases[currentPhase].GetNextPattern();
    }
}

[Serializable]
public class BossPhase
{
    public BossAttackPattern[] patterns;

    int patternIndex;

    public BossAttackPattern GetNextPattern()
    {
        if (patterns == null || patterns.Length == 0) return null;

        BossAttackPattern pattern = patterns[patternIndex];
        patternIndex = (patternIndex + 1) % patterns.Length;
        return pattern;
    }

    public void CancelCurrent()
    {
        if (patterns == null || patterns.Length == 0) return;

        int cur = (patternIndex - 1 + patterns.Length) % patterns.Length;
        patterns[cur]?.Cancel();
    }

    public void Reset() => patternIndex = 0;
}
