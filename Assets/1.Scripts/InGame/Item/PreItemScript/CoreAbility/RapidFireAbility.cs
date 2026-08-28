using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

// 속사 - 연속 발사
public class RapidFireAbility : SynergyAbility//, IComboAttack
{
    public int rapidCount = 1;

    CancellationTokenSource cts;

    public override void OnEquip(Character player)
    {
        cts = new CancellationTokenSource();
    }

    public override void OnUnequip(Character player)
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public async UniTask OnAttack(Character player, Vector2 dir)
    {
        for (int i = 0; i < rapidCount; i++)
        {
            await UniTask.Delay(Character.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
            Character.Instance.Shoot(new NormalBulletSpec(), dir);
        }
    }
}
