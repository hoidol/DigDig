using System.Threading;
using Cysharp.Threading.Tasks;

// 야생의 본능 - 적 처치 시 공격속도 증가 (최대 5중첩, 시간 지나면 해제)
public class WildInstinctAbility : Ability
{
    const int MAX_STACK = 5;
    float[] bonusPerStacks = { 0.08f, 0.1f, 0.13f };  // 중첩당 공격속도 8% 감소
    const float STACK_DURATION = 5f;

    int stackCount;
    Buff buff;
    CancellationTokenSource cts;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"적 처치 시 {100 * bonusPerStacks[c - 1]}% 만큼 공격 속도 증가 \n(최대 {MAX_STACK} 중첩)";
    }
    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackSpeed, 1f, StatOpType.Multiply);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        cts?.Cancel();
        player.RemoveBuff(buff);
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
        buff.value = 1f - bonusPerStacks[count - 1] * stackCount;
        Player.Instance.RemoveBuff(buff);
        Player.Instance.AddBuff(buff);
        Player.Instance.UpdatePlayer();
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
            Player.Instance.RemoveBuff(buff);
            Player.Instance.UpdatePlayer();
        }
    }
}
