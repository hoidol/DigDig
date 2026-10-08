using UnityEngine;
public class MeleeEnemy : NormalEnemy
{
    // MeleeAttackIndicator meleeAttackIndicator;
    // public Transform attackPoint;
    public override void Awake()
    {
        base.Awake();
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


        if (state == NormalEnemyState.Moving) UpdateMoving();
        else if (state == NormalEnemyState.Attack) UpdateAttack();
    }

    public override void UpdateAttack()
    {
        
        Character.Instance.TakeDamage(damageData);
        Destroy();
    }

}