using UnityEngine;
public class MeleeEnemy : Enemy
{
    MeleeAttackIndicator meleeAttackIndicator;
    public Transform attackPoint;
    protected override void Awake()
    {
        base.Awake();
        meleeAttackIndicator = GetComponentInChildren<MeleeAttackIndicator>();
    }

    public override void Spawn(Vector2 pos)
    {
        base.Spawn(pos);
        meleeAttackIndicator.gameObject.SetActive(false);
    }
    protected override void StartAttack()
    {
        base.StartAttack();

        meleeAttackIndicator.PlayIndicator(() =>
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(attackPoint.position, 1);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].CompareTag("Player"))
                {
                    Player.Instance.TakeDamage(enemyData.GetAttackPower(1));
                    break;
                }
            }
            EndAttack();
        });
    }

    public override void CancelAttack()
    {
        base.CancelAttack();
        meleeAttackIndicator.StopIndicator();
        EndAttack();
    }
}