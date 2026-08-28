using UnityEngine;
public class RangedEnemy : NormalEnemy
{
    // public Transform attackPoint;
    public override void StartAttack()
    {

        base.StartAttack();
        EnemyBulletObject enemyBullet = EnemyBulletObject.Instantiate();
        enemyBullet.transform.position = transform.position;
        enemyBullet.Shoot((Character.Instance.transform.position - transform.position).normalized,enemyData.GetAttackPower());
        
        EndAttack();
    }
}