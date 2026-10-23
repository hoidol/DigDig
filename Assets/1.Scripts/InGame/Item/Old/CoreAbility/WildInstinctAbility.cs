using System.Threading;
using Cysharp.Threading.Tasks;

// 야생의 본능 - 적 처치 시 공격속도 8% 증가 (최대 5중첩, 5초 유지)
public class WildInstinctAbility : Ability
{
    const float BONUS_PER_STACK = 0.08f;
    const int MAX_STACK = 5;
    const float STACK_DURATION = 5f;

    int stackCount;
    Buff buff;
    CancellationTokenSource cts;

    public override string GetDescription(bool detail = false)
    {
        return $"적 처치 시 공격속도 {BONUS_PER_STACK * 100}% 증가 (최대 {MAX_STACK}중첩, {STACK_DURATION}초 유지)";
    }

    public override void OnEquip(Character character)
    {
        buff = new Buff(StatType.AttackSpeed, 1f, StatOpType.Multiply);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnUnequip(Character character)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        cts?.Cancel();
        character.RemoveBuff(buff);
        stackCount = 0;
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (stackCount < MAX_STACK) stackCount++;
        RefreshBuff();
        ResetDecayTimer();
    }

    void RefreshBuff()
    {
        buff.value = 1f + BONUS_PER_STACK * stackCount;
        Character.Instance.RemoveBuff(buff);
        Character.Instance.AddBuff(buff);
        Character.Instance.UpdateCharacter();
    }

    void ResetDecayTimer()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        DecayLoop(cts.Token).Forget();
    }

    async UniTaskVoid DecayLoop(CancellationToken token)
    {
        while (stackCount > 0 && !token.IsCancellationRequested)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(STACK_DURATION), cancellationToken: token);
            if (token.IsCancellationRequested) return;
            stackCount = 0;
            Character.Instance.RemoveBuff(buff);
            Character.Instance.UpdateCharacter();
        }
    }
}
