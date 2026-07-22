using UnityEngine;
public class RangedEnemy : NormalEnemy
{
    public Transform attackPoint;
    public override void StartAttack()
    {

        base.StartAttack();
        EnemyBullet enemyBullet = EnemyBullet.Instantiate();
        enemyBullet.transform.position = transform.position;
        enemyBullet.Shoot((Player.Instance.transform.position - transform.position).normalized);
        enemyBullet.damage = enemyData.GetAttackPower();
        EndAttack();
    }
}