using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//처치 시 한발 더 발사함
public class DopamineSlime : SlimeGrowth1
{
    CancellationTokenSource cts;
    public float coolTime;
    public float coolTimer;

    public override void OnEnable()
    {
        base.OnEnable();
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);

        cts = new CancellationTokenSource();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);

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

        for (int i = 0; i < level+1; i++)
        {
            Fire(AttackDirecton());
            await UniTask.Delay(Character.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
        }
        coolTimer = coolTime;
    }

    public override void Update()
    {
        base.Update();
        if (coolTimer > 0)
        {
            coolTimer -= Time.deltaTime;
        }
    }

    public override string GetDescription(int level =0)
    {
        throw new System.NotImplementedException();
    }

    
    AllyBulletSpec allyBulletSpec;

    public override void Awake()
    {
        base.Awake();
        attackPowers = new float[] { 6, 20, 30 };
        attackSpeeds = new float[] { 10, 20, 30 };
        allyBulletSpec = new AllyBulletSpec();
    }
    public override AllyBulletObject GetBullet()
    {
        return allyBulletSpec.Instantiate(this);
    }
}