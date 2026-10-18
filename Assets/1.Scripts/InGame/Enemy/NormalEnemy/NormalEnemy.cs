using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class NormalEnemy : Enemy
{
    public NormalEnemyState state { get; private set; } // 적 상태 - FSM 패턴
    public Vector3 HpUIPosition => hpPoint.position;

    // protected float attackTimer;
    // protected bool attacking;


    [SerializeField] protected bool moving;
    // protected float moveTimer;

    public Animator animator;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    public override void Spawn(Vector2 pos, int wave)
    {
        base.Spawn(pos, wave);
        // moveTimer = MOVE_SPEED;
        //apearTime 초 후에 등장
        moving = false;

        root.localRotation = Quaternion.identity;
        // damageData.damage = GetAttackPower();

        ChangeState(NormalEnemyState.Moving);
    }

    public void ChangeState(NormalEnemyState state)
    {
        this.state = state;
    }

    public override void Reward()
    {
        Character.Instance.AddCoin(enemyData.gold);
        CoinText.Instantiate().SetCoinText(transform.position, $"+{enemyData.gold}");
        base.Reward();
    }

    public override void TakeDamage(DamageData damage)
    {
        base.TakeDamage(damage);
        //일반형 - 직관적으로 데미지 입음
        root.DOKill();
        root.localRotation = Quaternion.identity;
        root.DOShakeRotation(0.2f, new Vector3(0f, 0f, 5f), 100, 90f, false).OnComplete(() =>
        {
            root.localRotation = Quaternion.identity;
        });
    }

    public override void Destroy()
    {
        base.Destroy();

        CancelAttack();
        ChangeState(NormalEnemyState.Dead);

    }
}

public enum NormalEnemyState
{
    // Idle,
    Moving,
    //Attack,
    Dead
}