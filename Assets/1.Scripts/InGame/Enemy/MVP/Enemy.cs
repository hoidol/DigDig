using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public abstract class Enemy : MonoBehaviour, IHittable
{
    //[SerializeField] protected EnemyView view;
    public EnemyType enemyType;

    //protected EnemyModel model;

    public float MaxHp { get; private set; }
    public float CurHp { get; private set; }
    public EnemyState State { get; private set; }
    public EnemyData Data { get; private set; }

    //public event Action<float, float> OnHpChanged;
    // 
    [SerializeField] Transform root;
    [SerializeField] Transform hpPoint;
    protected Rigidbody2D rg2d;
    protected float attackTimer;
    protected bool attacking;

    public Transform Transform => transform;
    public EnemyData enemyData;
    HpUI hpUI;
    protected virtual void Awake()
    {
        rg2d = GetComponent<Rigidbody2D>();
    }
    public virtual void Init(EnemyData data)
    {
        enemyData = data;
    }

    public virtual void Spawn(Vector2 pos)
    {
        gameObject.SetActive(true);
        transform.position = pos;
        ChangeState(EnemyState.Approaching);

        attackTimer = 0;
        attacking = false;
    }

    public void ChangeState(EnemyState state)
    {
        State = state;
    }


    void Update()
    {
        if (attacking) return;

        if (attackTimer < enemyData.attackSpeed)
            attackTimer += Time.deltaTime;

        if (State == EnemyState.Approaching) UpdateApproaching();
        else if (State == EnemyState.Attack) UpdateAttack();

        //World Canvas UI
        if (hpUI != null && hpUI.IsOwn(transform))
        {
            hpUI.transform.position = hpPoint.position;
        }
    }
    public void SetFacing(float dirX)
    {
        root.localScale = new Vector3(dirX >= 0 ? 1 : -1, 1, 1);
    }

    void UpdateApproaching()
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

    void UpdateAttack()
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

    // IHittable
    public void TakeDamage(float damage)
    {
        CurHp = Mathf.Max(0, CurHp - damage);
        OnHpChanged(CurHp, MaxHp);
        if (CurHp <= 0) OnDead();
    }
    void OnHpChanged(float cur, float max)
    {
        if (hpUI == null)
        {
            hpUI = HpUI.GetHpUI(this);
        }

        hpUI.transform.position = hpPoint.position;
        hpUI.SetRate(cur / max);
    }

    void OnDead()
    {
        hpUI?.Release();
        gameObject.SetActive(false);
        // 이벤트 발행 → 각 시스템이 알아서 처리
        GameEventBus.Publish(new EnemyDeadEvent(this));
    }
}

public enum EnemyState
{

    Approaching,
    Attack
}