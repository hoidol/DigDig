using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 쿨타임 → 활성화 → 지속시간 → 비활성화 → 쿨타임 반복
// ex) OrbitOrbEffect
public abstract class TriggerCycleItem : Item
{
    public float coolTime;
    public float activeTime;

    public float CoolTimer { get; private set; }
    public bool IsActive { get; private set; }

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
        if (IsActive)
            OnDeactivate();
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

            IsActive = true;
            OnActivate();

            await UniTask.Delay(System.TimeSpan.FromSeconds(activeTime), cancellationToken: token);
            if (token.IsCancellationRequested) return;

            IsActive = false;
            OnDeactivate();
        }
    }

    public abstract void OnActivate();
    public abstract void OnDeactivate();
}
