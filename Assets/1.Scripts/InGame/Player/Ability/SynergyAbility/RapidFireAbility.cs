using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

// 속사 - 연속 발사
public class RapidFireAbility : SynergyAbility, IComboAttackItem
{
    public int rapidCount = 1;

    CancellationTokenSource cts;

    public override void OnEquip(Player player)
    {
        cts = new CancellationTokenSource();
    }

    public override void OnUnequip(Player player)
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public async UniTask OnAttack(Player player, Vector2 dir)
    {
        for (int i = 0; i < rapidCount; i++)
        {
            await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
            Player.Instance.Attack(dir, false);
        }
    }
}
