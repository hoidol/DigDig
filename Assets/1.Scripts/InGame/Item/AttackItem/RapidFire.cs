using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class RapidFire : Item, IAttackItem
{
    public int rapidCount = 2;

    CancellationTokenSource cts;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        cts = new CancellationTokenSource();
    }
    public override void OnUnequip(Player player)
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public void OnAttack(Player player, Vector2 dir)
    {
        Fire(dir).Forget();
    }
    async UniTask Fire(Vector2 dir)
    {
        for (int i = 0; i < rapidCount; i++)
        {
            await UniTask.Delay(200, cancellationToken: cts.Token);
            Player.Instance.Attack(dir);
        }
    }
}
