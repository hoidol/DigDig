using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//적 저치 시 관통탄발사 (관통만)
//관통	아드레날린
public class CriticalItem : Item 
{
    public int[] pierceCounts = {5,6,7};
    public float coolTime =2;
    public float coolTimer =0;
    void OnEnable()
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }
    void Osable()
    {
        
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }
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
    }

    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        if(coolTimer > 0)
            return;

        Shoot().Forget();
    }
    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if(coolTimer > 0)
            return;
        Shoot().Forget();
    }
    

    public async UniTask Shoot()
    {
        if(coolTimer > 0)
            return;

        PierceBullet pierceBullet = new PierceBullet();
        pierceBullet.multiplyAtk =1;
        pierceBullet.pierceCount = pierceCounts[count-1];
        for(int i = 0; i < count; i++)
        {
            Player.Instance.weapon.Shoot(pierceBullet, Player.Instance.weapon.GetAttackDirection());
            await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
        }
        Player.Instance.AddHp(-(count * itemData.consumeHp));
        coolTimer =coolTime;
    }

    void Update()
    {
        if(coolTimer > 0)
        {
            coolTimer -= Time.deltaTime;
        }
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"적 처치 시 관통탄 추가 발사 쿨타임 {coolTime}초\n발사 당 체력 {itemData.consumeHp} 감소";
    }

}