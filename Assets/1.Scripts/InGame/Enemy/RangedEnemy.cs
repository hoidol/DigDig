using UnityEngine;

public class RangedEnemy : Enemy
{

    public override void StartAttack()
    {
        base.StartAttack();
        EnemyBullet enemyBullet = EnemyBullet.Instantiate();
        enemyBullet.transform.position = transform.position;
        enemyBullet.Shoot((Player.Instance.transform.position - transform.position).normalized, enemyData.GetAttackPower());
        EndAttack();
    }
}