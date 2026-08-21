using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//5초마다 바라보는 방향으로 Firelance 발사 count 에 따라서 연속으로 추가 발사 (시간 간격 짧게 있음 )
public class FirelanceItem : Item
{
    public Firelance firelancePrefab;
    public float triggerInterval = 5f;
    public float burstInterval = 0.2f;

    float duration = 5;
    float dp = 4;

    CancellationTokenSource cts;

    public override void OnEquip()
    {
        base.OnEquip();
        cts = new CancellationTokenSource();
        Loop(cts.Token).Forget();
    }

    public override void OnUnequip()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public override string GetDescription()
    {
        return $"{triggerInterval}초마다 불창 {count}연발 발사";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),triggerInterval,count);
    }

    async UniTaskVoid Loop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(triggerInterval), cancellationToken: token);
            if (token.IsCancellationRequested) return;

            for (int i = 0; i < count; i++)
            {
                Fire();

                if (i < count - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(burstInterval), cancellationToken: token);
            }
        }
    }

    void Fire()
    {
        Vector2 dir = Character.Instance.weapon.GetAttackDirection();

        Firelance firelance = Instantiate(firelancePrefab);
        firelance.transform.position = Character.Instance.transform.position;
        firelance.damage = Character.Instance.statMgr.AttackPower;
        firelance.duration = duration;
        firelance.dps = dp;
        firelance.Shoot(dir);

        Character.Instance.AddHp(-itemData.consumeHp);
    }
}
