using System;
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
    public Action triggerListener;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        cts = new CancellationTokenSource();
        Loop(cts.Token).Forget();

        triggerListener = null;
    }

    public override void OnUnequip(Player player)
    {
        cts?.Cancel();
        cts?.Dispose();
        triggerListener = null;
    }

    async UniTaskVoid Loop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await WaitCooldown(token);
            if (token.IsCancellationRequested) return;
            OnTrigger();
            await UniTask.Yield(token);
        }
    }

    protected virtual async UniTask WaitCooldown(CancellationToken token)
    {
        CoolTimer = coolTime;
        while (CoolTimer > 0)
        {
            await UniTask.Yield(token);
            CoolTimer -= Time.deltaTime;
        }
    }

    public virtual void OnTrigger()
    {
        triggerListener?.Invoke();
    }

}
