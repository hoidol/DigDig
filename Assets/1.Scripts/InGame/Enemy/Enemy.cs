using UnityEngine;
using System.Collections.Generic;
using System;

public class Enemy : MonoBehaviour, IHittable
{
    Rigidbody2D rg2d;
    public EnemyType enemyType;
    public EnemyData enemyData;
    public EnemyState enemyState;
    public Transform root;


    public Transform hpPoint;
    HpUI hpUI = null;

    public Transform Transform => transform;
    public float maxHp;
    public float curHp;
    public float attackTimer;

    List<Collider2D> overlapBuffer;
    ContactFilter2D overlapFilter;
    const float OverlapCheckInterval = 0.15f;
    float overlapCheckTimer;
    Vector2 cachedOreStonePush;
    public bool attacking;

    public virtual void Awake()
    {
        tag = "Enemy";
        root = transform.Find("Root");
        rg2d = GetComponent<Rigidbody2D>();
        enemyData = Resources.Load<EnemyData>($"EnemyData/{enemyType}");
        overlapBuffer = new List<Collider2D>(16);
        overlapFilter = new ContactFilter2D().NoFilter();
    }

    public virtual void Spawn(Vector2 pos)
    {
        transform.position = pos;
        maxHp = enemyData.GetHp(1);
        curHp = maxHp;
        hpUI = null;
        EnterState(EnemyState.Approaching);
    }


    public void EnterState(EnemyState eState)
    {
        enemyState = eState;

    }
    void Update()
    {
        if (hpUI != null && hpUI.gameObject.activeSelf)
            hpUI.transform.position = hpPoint.position;

        if (attacking)
            return;

        if (attackTimer < enemyData.attackSpeed)
            attackTimer += Time.deltaTime;

        if (enemyState == EnemyState.Approaching)
        {
            UpdateApproaching();
        }
        else if (enemyState == EnemyState.Attack)
        {
            UpdateAttack();
        }

    }

    public virtual void StartAttack()
    {
        attackTimer = 0;
        attacking = true;
    }
    public virtual void EndAttack()
    {

        attacking = false;
        EnterState(EnemyState.Approaching);
    }
    public virtual void UpdateAttack()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;
        float length = vec.magnitude;
        if (length > enemyData.attackRange)
        {
            EnterState(EnemyState.Approaching);
            return;
        }
        Vector2 dir = vec.normalized;

        if (dir.x > 0)
            root.localScale = new Vector3(1, 1, 1);
        else
            root.localScale = new Vector3(-1, 1, 1);

        rg2d.linearVelocity = Vector2.zero;
        if (attackTimer >= enemyData.attackSpeed)
        {
            StartAttack();
        }
    }


    public virtual void UpdateApproaching()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;
        float length = vec.magnitude;

        Vector2 dir = vec.normalized;

        if (dir.x > 0)
            root.localScale = new Vector3(1, 1, 1);
        else
            root.localScale = new Vector3(-1, 1, 1);

        overlapCheckTimer += Time.deltaTime;

        if (overlapCheckTimer >= OverlapCheckInterval)
        {
            overlapCheckTimer = 0f;
            overlapBuffer.Clear();
            Physics2D.OverlapCircle((Vector2)transform.position, 0.4f, overlapFilter, overlapBuffer);
            Vector2 totalPush = Vector2.zero;
            for (int i = 0; i < overlapBuffer.Count; i++)
            {
                Collider2D overlap = overlapBuffer[i];
                if (overlap.gameObject == gameObject || !overlap.CompareTag("OreStone")) continue;
                Vector2 pushDir = ((Vector2)transform.position - (Vector2)overlap.transform.position).normalized;
                totalPush += pushDir;
            }
            cachedOreStonePush = totalPush.sqrMagnitude > 0.01f ? totalPush.normalized * 2f : Vector2.zero;
        }

        if (length <= enemyData.attackRange && cachedOreStonePush == Vector2.zero)
        {
            EnterState(EnemyState.Attack);
            return;
        }

        rg2d.linearVelocity = dir * enemyData.moveSpeed + cachedOreStonePush;
    }

    public void TakeDamage(float damage)
    {
        curHp -= damage;
        DamageText damageText = DamageText.Instantiate();
        damageText.SetDamageText(hpPoint.transform.position, damage.ToString());
        if (hpUI == null || !ReferenceEquals(hpUI.hittable, this))
        {
            hpUI = HpUI.GetHpUI(this);
            hpUI.transform.position = hpPoint.position;
        }
        hpUI.SetRate(curHp / maxHp);
        if (curHp <= 0)
        {
            Dead();
        }
    }
    public void Dead()
    {
        gameObject.SetActive(false);
        hpUI.Release();
        hpUI = null;

        Ore ore = OreManager.Instance.GetOre();
        ore.Droped(transform.position, "0");
        ore.transform.position = transform.position;

        ExpText expText = ExpText.Instantiate();
        expText.SetExpText(transform.position, enemyData.exp);
        Player.Instance.AddExp(enemyData.exp);
    }
}

public enum EnemyState
{

    Approaching,
    Attack
}


