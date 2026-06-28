using UnityEngine;
public class MeleeEnemy : NormalEnemy
{
    MeleeAttackIndicator meleeAttackIndicator;
    public Transform attackPoint;
    public override void Awake()
    {
        base.Awake();
        meleeAttackIndicator = GetComponentInChildren<MeleeAttackIndicator>(true);
    }
    public override void Spawn(Vector2Int[,] indexArr)
    {
        base.Spawn(indexArr);
        meleeAttackIndicator.gameObject.SetActive(false);
    }
    public override void StartAttack()
    {
        base.StartAttack();
        meleeAttackIndicator.transform.right = Player.Instance.transform.position - transform.position;
        meleeAttackIndicator.PlayIndicator(1.5f, () =>
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(attackPoint.position, enemyData.attackRange, LayerMask.GetMask("PlayerSide"));
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].TryGetComponent<IHittable>(out var hittable))
                {
                    hittable.TakeDamage(damageData);
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