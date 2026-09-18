using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HealSlime : SlimeGrowth1
{
    AllyBulletSpec allyBulletSpec;

    CancellationTokenSource cts;
    HealSlimeData healSlimeData;
    public float healCoolTime;
    public float healAmount;
    public HealEnhance8Ability healEnhance8Ability;

    public override void Spawn(Vector2 pos, int lv)
    {
        healSlimeData = SlimeManager.Instance.GetSlimeData(key) as HealSlimeData;
        base.Spawn(pos, lv);

        HealLoop().Forget();
    }
    

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        healCoolTime = healSlimeData.healCooltimes[mergeLevel];
        healAmount = float.Parse(healSlimeData.GetUniqueStat(SlimeStatType.Heal).values[mergeLevel]);
        base.InitSlime();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        cts = new CancellationTokenSource();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        cts?.Cancel();
        cts?.Dispose();
    }
    async UniTask HealLoop()
    {
        while (!cts.IsCancellationRequested)
        {
            await UniTask.Delay(Mathf.RoundToInt(healCoolTime * 1000), cancellationToken: cts.Token);
            float totalAmount = healAmount;
            if (healEnhance8Ability.isActivate)
            {
                totalAmount += Character.Instance.statMgr.MaxHp * healEnhance8Ability.percent;
            }
            Character.Instance.AddHp(totalAmount);
        }
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();

        return allyBulletSpec.Instantiate(this);
    }

    

}
