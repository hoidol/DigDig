using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//적 저치 시 관통탄발사 (관통만)
//관통	아드레날린
public class CriticalItem : Item
{
    int pierceCounts =  5;
    float coolTime = 2;
    float coolTimer = 0;
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
        if (coolTimer > 0)
            return;

        Shoot().Forget();
    }
    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (coolTimer > 0)
            return;
        Shoot().Forget();
    }


    public async UniTask Shoot()
    {
        if (coolTimer > 0)
            return;

        PierceBulletSpec pierceBullet = new PierceBulletSpec();
        pierceBullet.pierceCount = pierceCounts;
        for (int i = 0; i < count; i++)
        {
            Character.Instance.weapon.Shoot(pierceBullet, Character.Instance.weapon.GetAttackDirection());
            await UniTask.Delay(Character.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
        }
        Character.Instance.AddHp(-(count * itemData.consumeHp));
        coolTimer = coolTime;
    }

    void Update()
    {
        if (coolTimer > 0)
        {
            coolTimer -= Time.deltaTime;
        }
    }

    public override string GetDescription()
    {
        return $"적 처치 시 관통탄 추가 발사 쿨타임 {coolTime}초\n발사 당 체력 {itemData.consumeHp} 감소";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),coolTime,itemData.consumeHp);
    }

}