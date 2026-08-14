using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class NormalEnemy : Enemy
{
    public NormalEnemyState state { get; private set; } // 적 상태 - FSM 패턴
    public Vector3 HpUIPosition => hpPoint.position;

    protected float attackTimer;
    protected bool attacking;


    [SerializeField] protected bool moving;
    protected float moveTimer;

    public Animator animator;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    public override void Spawn(Vector2 pos)
    {
        base.Spawn(pos);
        moveTimer = MOVE_SPEED;
        //apearTime 초 후에 등장
        moving = false;

        root.localRotation = Quaternion.identity;
        damageData.damage = enemyData.GetAttackPower();

        ChangeState(NormalEnemyState.Moving);

        attackTimer = 0;
        attacking = false;
    }

    public async UniTask ChangeState(NormalEnemyState state)
    {
        this.state = state;
        if (state == NormalEnemyState.Moving)
        {
            // StartMoving().Forget();
        }
    }

    public override void Update()
    {
        if (!GameManager.Instance.isPlaying)
        {
            rg2d.linearVelocity = Vector2.zero;
            return;
        }

        base.Update();
            
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

        
        if (state == NormalEnemyState.Moving) UpdateMoving();
        else if (state == NormalEnemyState.Attack) UpdateAttack();

    }

    //상태가 Waiting 인 경우 처리
    public virtual void UpdateMoving()
    {
        if(isPushing)
            return;
        
        Vector2 vec = Character.Instance.transform.position - transform.position;
        
        if (vec.magnitude <= enemyData.attackRange)
        {
            ChangeState(NormalEnemyState.Attack).Forget();
            return;
        }

        rg2d.linearVelocity = vec.normalized * enemyData.moveSpeed;
        UpdateFacing(vec);
    }

    // public virtual async UniTask StartMoving()
    // {
    //     Vector2Int[] dirs = FindPath(transform.position, Character.Instance.transform.position);

    //     for (int i = 0; i < dirs.Length; i++)
    //     {
    //         // Debug.Log($"현 위치 {tileIndexArr[0, 0]} 방향 {dirs[i]}");
    //         if (!MapManager.CheckMoveTo(tileIndexArr, dirs[i]))
    //             continue;

    //         if (moving) return;
    //         moving = true;
    //         await MoveTo(dirs[i]);

    //         moving = false;
    //         moveTimer = MOVE_SPEED;
    //         ChangeState(NormalEnemyState.Waiting).Forget();
    //         return;
    //     }

    //     //이동 못하는 경우
    //     ChangeState(NormalEnemyState.Waiting).Forget();
    //     moveTimer = MOVE_SPEED;
    // }

    public virtual void UpdateAttack()
    {
        
        if (attackTimer < enemyData.attackSpeed)
            return;
        
        if (!attacking)
        {
            Vector2 vec = Character.Instance.transform.position - transform.position;
            UpdateFacing(vec);
            StartAttack();
        }
    }

    public virtual void StartAttack()
    {
        attacking = true;
        rg2d.linearVelocity = Vector2.zero;
        
        attackTimer = 0;
    }
    public override void Reward()
    {
        OrePiece.Instantiate(transform.position,1,1);
        base.Reward();
    }
    public override void TakeDamage(DamageData damage)
    {
        base.TakeDamage(damage);
        root.DOKill();
        root.localRotation = Quaternion.identity;
        root.DOShakeRotation(0.2f, new Vector3(0f, 0f, 5f), 100, 90f, false).OnComplete(() =>
        {
            root.localRotation = Quaternion.identity;
        });
    }
    public virtual void EndAttack()
    {
        attacking = false;
        ChangeState(NormalEnemyState.Moving).Forget();
    }

    public override void Destroy()
    {
        base.Destroy();
        CancelAttack();
        ChangeState(NormalEnemyState.Dead).Forget();

    }
}

public enum NormalEnemyState
{
    Moving,
    Attack,
    Dead
}