using System;
using UnityEngine;

[RequireComponent(typeof(MiniMe))]
public class MiniMeAttackBehaviour : MonoBehaviour 
{

    public MiniMe miniMe;

    public float attackTimer;

    public Transform targetTr;

    public Action<Transform> onTargetListener;
    
    public virtual void Awake()
    {
        miniMe = GetComponent<MiniMe>();
    }

    void Update()
    {
        
        attackTimer += Time.deltaTime;
        if (attackTimer > miniMe.AttackSpeed())
        {
            Fire(AttackDirecton());
        }
    }


    public virtual Vector2 AttackDirecton()
    {
        targetTr = InGameUtil.FindTarget(transform.position, 10, miniMe.targetLayerMask);
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
        AllyBulletObject baseBullet = miniMe.GetBullet();
        baseBullet.transform.position = transform.position;
        baseBullet.Shoot(dir,miniMe.AttackPower());
        attackTimer = 0;
    }

    
}