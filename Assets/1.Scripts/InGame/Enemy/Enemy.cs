using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public abstract class Enemy : MonoBehaviour, IHittable
{
    public EnemyType enemyType; // 적 종류 구분
    public EnemyState state { get; private set; } // 적 상태 - FSM 패턴
    public EnemyData enemyData { get; private set; } //게임 데이터
    public StatusEffectHandler statusEffectHandler;
    // {
    //     get; set;
    // }

    #region 
    public float MaxHp { get; private set; }
    public float CurHp { get; private set; }

    [SerializeField] Transform root;
    [SerializeField] Transform hpPoint;
    protected Rigidbody2D rg2d;
    public Rigidbody2D Rigidbody2D => rg2d;
    protected float attackTimer;
    protected bool attacking;

    public Transform Transform => transform;
    HpUI hpUI;
    #endregion
    protected virtual void Awake()
    {
        rg2d = GetComponent<Rigidbody2D>();
        statusEffectHandler = GetComponent<StatusEffectHandler>();
    }
    //Enemy 게임 데이터 설정
    public virtual void Init(EnemyData data)
    {
        enemyData = data;
        statusEffectHandler.Init();
    }
    //적 생성 시 호출
    public virtual void Spawn(Vector2 pos)
    {
        gameObject.SetActive(true);
        transform.position = pos;
        MaxHp = enemyData.GetHp();
        CurHp = MaxHp;
        ChangeState(EnemyState.Approaching);

        attackTimer = 0;
        attacking = false;
    }

    //상태 전환
    public void ChangeState(EnemyState state)
    {
        this.state = state;
    }


    void Update()
    {
        if (statusEffectHandler.IsStunned)
        {
            if (attacking)
            {
                CancelAttack();
            }
            return;
        }

        if (attackTimer < enemyData.attackSpeed)
            attackTimer += Time.deltaTime;
        else
        {
            if (attacking)
                return;
        }

        if (state == EnemyState.Approaching) UpdateApproaching();
        else if (state == EnemyState.Attack) UpdateAttack();

        //World Canvas UI
        if (hpUI != null && hpUI.IsOwn(transform))
        {
            hpUI.transform.position = hpPoint.position;
        }
    }

    public virtual void CancelAttack()
    {

    }

    //상태가 Approaching 인 경우 처리
    public virtual void UpdateApproaching()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;
        if (vec.magnitude <= enemyData.attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        SetFacing(vec.x);
        rg2d.linearVelocity = vec.normalized * enemyData.moveSpeed;
    }

    //상태가 Attack 인 경우 처리
    public virtual void UpdateAttack()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;
        SetFacing(vec.x);
        rg2d.linearVelocity = Vector2.zero;

        if (attackTimer >= enemyData.attackSpeed)
            StartAttack();
    }


    protected virtual void StartAttack()
    {
        attackTimer = 0;
        attacking = true;
    }

    protected void EndAttack()
    {
        attacking = false;
        ChangeState(EnemyState.Approaching);
    }

    // IHittable 인터페이스 구현 부
    public void TakeDamage(DamageData damage)
    {
        if (state == EnemyState.Dead)
            return;
        damage.Applyed(hpPoint.transform.position);
        CurHp = Mathf.Max(0, CurHp - damage.damage);
        OnHpChanged(CurHp, MaxHp);
        if (CurHp <= 0) OnDead(damage);
    }

    public void SetFacing(float dirX)
    {
        root.localScale = new Vector3(dirX >= 0 ? 1 : -1, 1, 1);
    }

    protected virtual void OnHpChanged(float cur, float max)
    {
        if (hpUI == null)
        {
            hpUI = HpUI.GetHpUI(this);
        }

        hpUI.transform.position = hpPoint.position;
        hpUI.SetRate(cur / max);
    }

    protected virtual void OnDead(DamageData damage)
    {
        ChangeState(EnemyState.Dead);
        hpUI?.Release();
        gameObject.SetActive(false);
        // 이벤트 발행 → 각 시스템이 알아서 처리
        GameEventBus.Publish(new EnemyDeadEvent(this, damage.cause));
    }
}

public enum EnemyState
{
    Approaching,
    Attack,
    PhaseTransition,
    Dash,
    Dead
}