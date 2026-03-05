using UnityEngine;

public class RangedEnemy : Enemy
{

    public override void Attack()
    {
        base.Attack();
        EnemyBullet enemyBullet = EnemyBullet.Instantiate();
        enemyBullet.transform.position = transform.position;
        enemyBullet.Shoot((Player.Instance.transform.position - transform.position).normalized, enemyData.GetAttackPower());
    }
}