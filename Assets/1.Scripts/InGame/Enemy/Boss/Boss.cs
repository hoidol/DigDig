using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Linq;


public abstract class Boss : Enemy, IEnemySpecialAttackPattern
{
    public BossState bossState;

    // 보스가 쓰는 스킬 (자식의 EnemyAttackPattern). 체력에 따른 페이즈 없음: 어떤 스킬을 쓸지는 StartBehaviour가 정함
    EnemyAttackPattern[] attackPatterns;
    public float waitingTime = 5f;

    [SerializeField] BossBehaviour[] bossBehaviours;
    // public BossBehaviour curBossBehaviour;

    public override void Awake()
    {
        base.Awake();
        bossBehaviours = GetComponentsInChildren<BossBehaviour>();
        attackPatterns = GetComponentsInChildren<EnemyAttackPattern>(true);
    }


    public override void Spawn(Vector2 pos, int wave)
    {
        EnterBossState(BossState.Intro);
        base.Spawn(pos, wave);
        GameEventBus.Publish(new BossSpawnEvent(this));
    }

    public override void Apear()
    {
        base.Apear();
        StartIntro().Forget();

    }
    async UniTask StartIntro()
    {
        await UniTask.Delay(2000);
        EnterBossState(BossState.Beviouring);
    }

    public void EnterBossState(BossState bossState)
    {
        this.bossState = bossState;
        if (bossState == BossState.Beviouring)
        {
            StartBehaviour().Forget();
        }
    }

    public abstract UniTask StartBehaviour();

    public virtual async UniTask StartWait()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        EnterBossState(BossState.Beviouring);
    }


    public override void Destroy()
    {
        base.Destroy();
        GameEventBus.Publish(new BossDeadEvent(this));
    }

    public EnemyAttackPattern GetAttackPattern(string key)
    {
        for (int i = 0; i < attackPatterns.Length; i++)
        {
            if (attackPatterns[i].key == key)
                return attackPatterns[i];
        }
        return null;
    }

    public virtual float PlayAnim(string animName)
    {
        return 0;
    }


    public override void CancelAttack()
    {
    }

    //부수면서 이동하기
    // public async override UniTask MoveTo(Vector2Int dir, float delaySec = 2)
    // {
    //     Vector2Int[,] newTiles = MapManager.GetIndexArray(tileIndexArr, dir);

    //     for (int i = 0; i < newTiles.GetLength(0); i++)
    //     {
    //         for (int j = 0; j < newTiles.GetLength(1); j++)
    //         {
    //             if (!MapManager.CheckEmpty(newTiles[i, j]))
    //             {
    //                 if (MapManager.tileArray[newTiles[i, j].x, newTiles[i, j].y] == this)
    //                 {
    //                     continue;
    //                 }
    //                 MapManager.tileArray[newTiles[i, j].x, newTiles[i, j].y].Destroy();
    //             }
    //         }
    //     }
    //     await base.MoveTo(dir, delaySec);
    // }
    protected override void ReachPathEnd()
    {
        rg2d.linearVelocity = Vector2.zero;
        damageData.damage = 99999;
        KingSlime.Instance.TakeDamage(damageData);

    }
    public BossBehaviour GetBossBehaviour(string name)
    {
        return bossBehaviours.Where(e => e.behaviourName == name).FirstOrDefault();
    }
}

public enum BossState
{
    Intro,//등장 연출
    Beviouring,//행동
}