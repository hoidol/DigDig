using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//4회마다 관통 화염탄
public class FirelanceItem : Item, IFired, IComboFire
{
    public int triggerCount = 5;
    public Firelance firelancePrefab;
    int triggerCounter;
    CancellationTokenSource cts;
    public float damage;
    public float[] durations ={5,5,5};
    public float[] dps = {5,6,7};
    public override void OnEquip()
    {
        base.OnEquip();
        cts = new CancellationTokenSource();
    }

    public override string GetDescription(int lv, bool detail = false)
    {
        return $"{triggerCount}발사마다 불차 추가 발사";
    }

    public override void OnUnequip()
    {
        cts?.Cancel();
        cts?.Dispose();
        triggerCounter=0;
    }

    public void OnFired(ref Bullet bullet, ref PlayerBulletObject playerBulletObject, Vector2 dir)
    {
        triggerCounter++;       
    }

    public async UniTask OnComboFire(Vector2 dir)
    {
        if(triggerCounter < triggerCount)
            return;
            
        await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);

        Firelance firelance = Instantiate(firelancePrefab);
        firelance.damage = Player.Instance.statMgr.AttackPower;
        firelance.duration = durations[count-1];
        firelance.dps = dps[count-1];
        firelance.Shoot(dir);

        Player.Instance.AddHp(-itemData.consumeHp);
        triggerCounter=0;
    }
}