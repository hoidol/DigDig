using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
public abstract class Enemy : MonoBehaviour, IHittable
{
    public EnemyType enemyType; // 적 종류 구분
    public EnemyData enemyData; //게임 데이터
    [field: SerializeField]
    public StatusEffectHandler statusEffectHandler
    {

        get; private set;
    }
    public float MaxHp => maxHp;
    [SerializeField] public float maxHp;//{ get; private set; }
    public float CurHp => curHp;
    [field: SerializeField] public float curHp;// { get; private set; }

    [SerializeField] protected Transform root;
    [SerializeField] protected Transform hpPoint;
    protected Rigidbody2D rg2d;
    public Rigidbody2D Rigidbody2D => rg2d;
    protected Collider2D col2d;


    public Transform Transform => transform;


    public float apearTime = 2; //떨어지면서 등장하는 시간
    public const float MOVE_SPEED = 2; //떨어지면서 등장하는 시간

    public EnemyDamageData damageData = new EnemyDamageData();
    public TMP_Text hpText;

    public virtual void Awake()
    {
        rg2d = GetComponent<Rigidbody2D>();
        col2d = GetComponent<Collider2D>();
        statusEffectHandler = GetComponent<StatusEffectHandler>();
        statusEffectHandler?.Init();
    }
    //적 생성 시 호출
    public virtual void Spawn(Vector2 pos)
    {
        gameObject.SetActive(gameObject);

        Apear();
        transform.position = pos;
        maxHp = enemyData.GetHp();
        curHp = maxHp;
        hpText.text = ((int)curHp).ToString();

        damageData.damage = enemyData.GetAttackPower();
    }

    const float APEAR_POP_DURATION = 0.15f;
    public virtual void Apear()
    {
        gameObject.SetActive(true);

        if (col2d != null)
            col2d.enabled = false;

        transform.localScale = Vector3.zero;
        transform.DOScale(1f, APEAR_POP_DURATION).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (col2d != null)
                    col2d.enabled = true;
            });
    }


    public virtual void Update()
    {
        if (isPushing)
        {
            if (rg2d.linearVelocity.magnitude < 0.1f)
            {
                isPushing = false;
            }
        }

    }


    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (statusEffectHandler == null)
        {
            statusEffectHandler = GetComponent<StatusEffectHandler>();
            if (statusEffectHandler == null)
            {
                statusEffectHandler = gameObject.AddComponent<StatusEffectHandler>();
            }
        }
        statusEffectHandler.Apply(effect);
    }
    public virtual void CancelAttack()
    {

    }



    // IHittable 인터페이스 구현 부
    public virtual void TakeDamage(DamageData damage)
    {
        if (curHp <= 0)
            return;

        
        curHp = Mathf.Max(0, curHp - damage.ApplyDamage(hpPoint.transform.position));
        if (curHp > 1)
            hpText.text = ((int)curHp).ToString();
        else if (0 < curHp && curHp <= 1)
            hpText.text = "1";

        OnHpChanged();
        if (curHp < 1)
        {
            Reward();
            Destroy();
        }
    }

    public int face;
    public void UpdateFacing(Vector2 vec)
    {
        face = vec.x >= 0 ? 1 : -1;
        root.localScale = new Vector3(face, 1, 1);
    }


    protected virtual void OnHpChanged()
    {

    }
    public virtual void Reward()
    {
        //Exp.Instantiate(transform.position, enemyData.exp, 1);
        EffectManager.Instance.Play(EffectType.StoneBreak, transform.position);
    }
    public virtual void Destroy()
    {
        GameEventBus.Publish(new EnemyDeadEvent(this));
    }
    public bool CanHit()
    {
        return curHp > 0;
    }

    public bool isPushing;
    public void Push(Vector2 dir, float power)
    {
        isPushing = true;
        rg2d.AddForce(dir * power, ForceMode2D.Impulse);
    }

}
