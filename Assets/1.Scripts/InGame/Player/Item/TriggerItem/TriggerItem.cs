using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 쿨타임 → 발동 → 즉시종료 → 쿨타임 반복
// ex) Sword, Bandage, Shell, BlackBomb, BombGenerator, PiggyBank
public abstract class TriggerItem : Item
{
    public float coolTime;
    public float CoolTimer { get; set; }

    CancellationTokenSource cts;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        cts = new CancellationTokenSource();
        Loop(cts.Token).Forget();
    }

    public override void OnUnequip(Player player)
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    async UniTaskVoid Loop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            CoolTimer = coolTime;
            while (CoolTimer > 0)
            {
                await UniTask.Yield(token);
                CoolTimer -= Time.deltaTime;
            }
            if (token.IsCancellationRequested) return;
            OnTrigger();
        }
    }

    public abstract void OnTrigger();
}
