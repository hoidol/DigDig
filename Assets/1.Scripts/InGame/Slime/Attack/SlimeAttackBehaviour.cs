using System;
using UnityEngine;

[RequireComponent(typeof(Slime))]
public class SlimeAttackBehaviour : MonoBehaviour 
{

    public Slime slime;

    public float attackTimer;

    public Transform targetTr;

    public Action<Transform> onTargetListener;
    
    public virtual void Awake()
    {
        slime = GetComponent<Slime>();
    }

    void Update()
    {
        
        attackTimer += Time.deltaTime;
        if (attackTimer > slime.AttackSpeed())
        {
            Fire(AttackDirecton());
        }
    }


    public virtual Vector2 AttackDirecton()
    {
        targetTr = slime.FindTarget();
        onTargetListener?.Invoke(targetTr);

        Vector2 fireDir = Character.Instance.moveJoystick.Direction;
        if (targetTr != null)
        {
            fireDir = (targetTr.position - transform.position).normalized;

        }

        return fireDir;
    }


    
    public virtual void Fire(Vector2 dir)
    {
        AllyBulletObject baseBullet = slime.GetBullet();
        if(baseBullet == null)
            return;
        baseBullet.transform.position = transform.position;
        baseBullet.Shoot(dir,slime.AttackPower());
        attackTimer = 0;
    }

    
}