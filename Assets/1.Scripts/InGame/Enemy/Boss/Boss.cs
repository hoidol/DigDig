using UnityEngine;
using System;
using Cysharp.Threading.Tasks;


public abstract class Boss : Enemy, IEnemySpecialAttackPattern
{
    public BossState bossState;
    protected int currentPhase;
    public int CurrentPhase => currentPhase;

    [SerializeField] BossPhase[] phases;
    public BossPhase curBossPhase;
    public float waitingTime = 5f;

    [SerializeField] BossBehaviour[] bossBehaviours;
    public BossBehaviour curBossBehaviour;

    public override void Awake()
    {
        base.Awake();
        bossBehaviours = GetComponentsInChildren<BossBehaviour>();
    }


    public override void Spawn(Vector2Int[,] idxArr)
    {
        for (int i = 0; i < phases.Length; i++)
        {
            phases[i].Init(this);
        }
        EnterBossState(BossState.Intro);
        currentPhase = 0;
        base.Spawn(idxArr);
        GameEventBus.Publish(new BossSpawnEvent(this));
    }

    public override void Apear()
    {
        base.Apear();
        OnEnterPhase(0);
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
        if (bossState == BossState.Waiting)
        {
            StartWait().Forget();
        }
        else if (bossState == BossState.Beviouring)
        {
            StartBehaviour().Forget();
        }
    }

    async UniTask StartBehaviour()
    {
        curBossBehaviour = bossBehaviours[UnityEngine.Random.Range(0, bossBehaviours.Length)];
        await curBossBehaviour.StartBehaviour();
        EnterBossState(BossState.Waiting);
    }
    async UniTask StartWait()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(waitingTime));
        EnterBossState(BossState.Beviouring);
    }


    public override void Update()
    {
        if (bossState == BossState.Intro)
            return;
    }

    protected override void OnHpChanged()
    {
        base.OnHpChanged();
        CheckPhaseTransition();
    }

    public override void OnDestroy()
    {
        if (curBossPhase != null)
            curBossPhase.EndPhase();

        base.OnDestroy();
        GameEventBus.Publish(new BossDeadEvent(this));
    }

    void CheckPhaseTransition()
    {
        float hpRate = curHp / maxHp;
        int newPhase = 0;
        for (int i = 0; i < phases.Length; i++)
        {
            if (hpRate <= phases[i].phaseThreshold)
                newPhase = i + 1;
        }

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            GameEventBus.Publish(new BossPhaseChangedEvent(this, currentPhase));
            OnEnterPhase(currentPhase);
        }
    }

    protected virtual void OnEnterPhase(int phase)
    {
        if (curBossPhase != null)
            curBossPhase.EndPhase();

        curBossPhase = phases[phase];
        curBossPhase.StartPhase();
    }

    public virtual float PlayAnim(string animName)
    {
        return 0;
    }


    public override void CancelAttack()
    {
        if (phases != null && currentPhase < phases.Length)
            phases[currentPhase].EndPhase();

    }

    //부수면서 이동하기
    public async override UniTask MoveTo(Vector2Int dir, float delaySec = 2)
    {
        Vector2Int[,] newTiles = MapManager.GetIndexArray(tileIndexArr, dir);

        for (int i = 0; i < newTiles.GetLength(0); i++)
        {
            for (int j = 0; j < newTiles.GetLength(1); j++)
            {
                if (!MapManager.CheckEmpty(newTiles[i, j]))
                {
                    if (MapManager.tileArray[newTiles[i, j].x, newTiles[i, j].y] == this)
                    {
                        continue;
                    }
                    MapManager.tileArray[newTiles[i, j].x, newTiles[i, j].y].OnDestroy();
                }
            }
        }
        await base.MoveTo(dir, delaySec);
    }

}

public enum BossState
{
    Intro,//등장 연출
    Beviouring,//행동
    Waiting// 공격 대기 + 휴식
}