using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//미니미 2개 소환 및 {durations[lv-1]}동안 플레이어 + 플레이어 공격 속도 증가\n쿨타임 : {coolTimes[lv-1]}초, 발사 당 체력 -{itemData.consumeHp}
//4번 마다 폭발탄 발사(범위 피해) - 오차있게 발사 
public class BoomItem : Item, IFired, IComboFire
{
    public int triggerCount = 5;
    
    int triggerCounter;
    CancellationTokenSource cts;
    public override void OnEquip()
    {
        base.OnEquip();
        cts = new CancellationTokenSource();
    }

    // public override string GetDescription(int lv, bool detail = false)
    // {
    //     return $"{triggerCount}발사마다 폭탄 발사\n관통력 +{pierceCounts[lv-1]}";
    // }

    public override void OnUnequip()
    {
        cts?.Cancel();
        cts?.Dispose();
        triggerCounter=0;
    }

    public void OnFired(Vector2 dir)
    {
        triggerCounter++;       
       
    }

    public async UniTask OnComboFire(Vector2 dir)
    {
        
        if(triggerCounter < triggerCount)
            return;
            
        await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);

        BoomBullet boomBullet = new BoomBullet();
        float angleOffset = Random.Range(-10f, 10f) * Mathf.Deg2Rad;
        Vector2 shootDir = new Vector2(
            dir.x * Mathf.Cos(angleOffset) - dir.y * Mathf.Sin(angleOffset),
            dir.x * Mathf.Sin(angleOffset) + dir.y * Mathf.Cos(angleOffset));
        Player.Instance.Shoot(boomBullet, shootDir);

        Player.Instance.AddHp(-itemData.consumeHp);

        triggerCounter=0;
    }
}