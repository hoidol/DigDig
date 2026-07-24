using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//적 처치 시 즉시 발사 - 쿨타임 2초
public class RushItem : Item 
{
    public float coolTime =3;
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

        for(int i = 0; i < count; i++)
        {
            Player.Instance.weapon.Shoot(null, Player.Instance.weapon.GetAttackDirection());
            await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
        }
        Player.Instance.AddHp(-count);
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
        return $"적 처치 시 즉시 탄 발사 쿨타임 {coolTime}초\n발사 당 체력 {itemData.consumeHp} 감소";
    }

}


// float[] coolTimes = {10,8,6};
//     float[] attackSpeeds = {1.6f,1.9f,2.2f};
//     float[] durations = {3f,4f,5f};

//     Buff attackSpeedBuff;

//     public override void UpdateItem()
//     {
//         base.UpdateItem();
//         activeTime = durations[count-1];
//         coolTime = coolTimes[count-1];
//     }
    
//     public override void OnUnequip()
//     {
//         base.OnUnequip();

//         if(attackSpeedBuff != null)
//             Player.Instance.RemoveBuff(attackSpeedBuff);
            
//         attackSpeedBuff = null;
//     }

//     public override void OnActivate()
//     {
//         attackSpeedBuff = new Buff( StatType.AttackSpeed,attackSpeeds[count-1], StatOpType.Multiply);
//         Player.Instance.AddBuff(attackSpeedBuff);
//     }

//     public override void OnDeactivate()
//     {
//         if(attackSpeedBuff != null)
//             Player.Instance.RemoveBuff(attackSpeedBuff);

//         attackSpeedBuff = null;
//     }

//     public override string GetDescription(int lv = 1,bool detail = false)
//     {
//         return $"{coolTimes[lv-1]}초마다 {durations[count-1]}초 동안 공격 속도 +{(int)((attackSpeeds[lv-1]-1)*100)}% 증가";
//     }