using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//4회마다 연속 발사
public class ChainItem : Item, IFired, IComboFire
{
    int[] triggerCounts = { 2, 2, 2 };
    int[] shootCounts = { 1, 2, 3 };
    int triggerCounter;
    bool active;

    CancellationTokenSource cts;
    Bullet bullet;

    public override void OnEquip()
    {
        base.OnEquip();
        cts = new CancellationTokenSource();
    }

    public override void OnUnequip()
    {
        cts?.Cancel();
        cts?.Dispose();
        active = false;
        triggerCounter = 0;
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"{triggerCounts[lv - 1]}회 발사마다 연속 발사";
    }

    public async UniTask OnComboFire(Vector2 dir)
    {
        if (!active)
            return;

        active = false;
        for (int i = 0; i < shootCounts[count - 1]; i++)
        {
            await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
            Player.Instance.Shoot(bullet, dir);
        }

        Player.Instance.AddHp(-itemData.consumeHp);
        triggerCounter = 0;


    }

    public void OnFired(ref Bullet bullet, ref PlayerBulletObject playerBulletObject, Vector2 dir)
    {

        triggerCounter++;
        active = triggerCounter >= triggerCounts[count - 1];
        this.bullet = bullet;

    }

}