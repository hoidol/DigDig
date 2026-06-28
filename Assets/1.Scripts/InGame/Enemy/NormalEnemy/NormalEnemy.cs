using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class NormalEnemy : Enemy, IHpUI
{
    public NormalEnemyState state { get; private set; } // 적 상태 - FSM 패턴
    public float MaxHp => maxHp;
    public float CurHp => curHp;
    public Vector3 HpUIPosition => hpPoint.position;

    protected float attackTimer;
    protected bool attacking;

    HpUI hpUI;

    [SerializeField] protected bool moving;
    protected float moveTimer;

    public Animator animator;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    public override void Spawn(Vector2Int[,] idxArr)
    {
        base.Spawn(idxArr);
        moveTimer = MOVE_SPEED;
        //apearTime 초 후에 등장
        moving = false;


        damageData.damage = enemyData.GetAttackPower();

        ChangeState(NormalEnemyState.Waiting);

        attackTimer = 0;
        attacking = false;
    }

    public async UniTask ChangeState(NormalEnemyState state)
    {
        this.state = state;
        if (state == NormalEnemyState.Moving)
        {
            StartMoving().Forget();
        }
    }

    public override void Update()
    {
        if (statusEffectHandler.IsStunned)
        {
            if (attacking)
            {
                CancelAttack();
            }
            return;
        }

        if (moveTimer > 0)
            moveTimer -= Time.deltaTime;


        if (attackTimer < enemyData.attackSpeed)
            attackTimer += Time.deltaTime;

        if (attacking)
            return;

        if (state == NormalEnemyState.Waiting) UpdateWaiting();
        else if (state == NormalEnemyState.Attack) UpdateAttack();

    }

    //상태가 Waiting 인 경우 처리
    public virtual void UpdateWaiting()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;

        if (vec.magnitude > enemyData.moveRange && moveTimer <= 0)
        {
            ChangeState(NormalEnemyState.Moving).Forget();
            return;
        }
        if (vec.magnitude <= enemyData.attackRange)
        {
            ChangeState(NormalEnemyState.Attack).Forget();
            return;
        }
        SetFacing(vec.x);

    }
    public virtual async UniTask StartMoving()
    {
        Vector2Int[] dirs = FindPath(transform.position, Player.Instance.transform.position);

        for (int i = 0; i < dirs.Length; i++)
        {
            // Debug.Log($"현 위치 {tileIndexArr[0, 0]} 방향 {dirs[i]}");
            if (!MapManager.CheckMoveTo(tileIndexArr, dirs[i]))
                continue;

            if (moving) return;
            moving = true;
            await MoveTo(dirs[i]);

            moving = false;
            moveTimer = MOVE_SPEED;
            ChangeState(NormalEnemyState.Waiting).Forget();
            return;
        }

        //이동 못하는 경우
        ChangeState(NormalEnemyState.Waiting).Forget();
        moveTimer = MOVE_SPEED;
    }

    public virtual void UpdateAttack()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;
        SetFacing(vec.x);
        rg2d.linearVelocity = Vector2.zero;

        if (attackTimer >= enemyData.attackSpeed)
            StartAttack();
    }





    protected override void OnHpChanged()
    {
        if (hpUI == null || !hpUI.IsOwn(this))
            hpUI = HpUI.Get(this);
        hpUI.UpdateTime();
    }

    public virtual void StartAttack()
    {
        attacking = true;
        attackTimer = 0;
    }

    public virtual void EndAttack()
    {
        attacking = false;
        ChangeState(NormalEnemyState.Waiting).Forget();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        ChangeState(NormalEnemyState.Dead).Forget();
        hpUI?.Release();

    }
}

public enum NormalEnemyState
{
    Waiting,
    Moving,
    Attack,
    Dead
}