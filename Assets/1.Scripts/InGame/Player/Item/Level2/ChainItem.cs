using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
 //4회마다 연속 발사
public class ChainItem : Item, IComboFire
{
    int[] triggerCounts = {4,3,2};
    int triggerCounter;
    bool active;

    CancellationTokenSource cts;

    public override void OnEquip()
    {
        base.OnEquip();
        cts = new CancellationTokenSource();
    }

    public override void OnUnequip()
    {
        cts?.Cancel();
        cts?.Dispose();
        active= false;
        triggerCounter=0;
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{triggerCounts[lv-1]}회 발사마다 연속 발사";
    }

    public async UniTask OnComboFire(Vector2 dir)
    {
        if(!active)
            return;

        active= false;
        await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
        Player.Instance.Shoot(new NormalBullet(), dir);
        Player.Instance.AddHp(-itemData.consumeHp);
        triggerCounter=0;
        
        
    }

    public void OnFired(Vector2 dir)
    {
        triggerCounter++;        
        active =triggerCounter >= triggerCounts[count-1];
        
        
    }
}