using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HealSlime : SlimeGrowth1
{

    AllyBulletSpec allyBulletSpec;
    float healCooltime = 10f    ;
    float[] healAmounts = {1f,2,3f};

    CancellationTokenSource cts;

    public override void Awake()
    {
        base.Awake();
        UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);

        userSlime.EnhanceLevel();

        attackPowers = new float[] {4,6,8};
        attackSpeeds = new float[] {2,2,2};


        allyBulletSpec = new AllyBulletSpec();
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

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);
        HealLoop().Forget();
    }

    async UniTask HealLoop()
    {
        while (!cts.IsCancellationRequested)
        {
            await UniTask.Delay(Mathf.RoundToInt(healCooltime * 1000), cancellationToken: cts.Token);
            Character.Instance.AddHp(healAmounts[level]);
        }
    }

    public override AllyBulletObject GetBullet()
    {
        return allyBulletSpec.Instantiate(this);
    }

    public override string GetDescription(int level =0)
    {
        return $"{healCooltime}초마다 체력 {healAmounts[level]}씩 회복합니다.";
    }
}
