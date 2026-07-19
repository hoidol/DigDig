using UnityEngine;
public class MeleeEnemy : NormalEnemy
{
    // MeleeAttackIndicator meleeAttackIndicator;
    public Transform attackPoint;
    public float realAttackRange = 1.5f;
    public override void Awake()
    {
        base.Awake();
        // meleeAttackIndicator = GetComponentInChildren<MeleeAttackIndicator>(true);
    }
    public override void Spawn(Vector2Int[,] indexArr)
    {
        base.Spawn(indexArr);
        // meleeAttackIndicator.gameObject.SetActive(false);
    }
    WarningIndicator warningIndicator;
    public override void StartAttack()
    {
        base.StartAttack();

        Vector2 aPoint = transform.position + (Player.Instance.transform.position - transform.position).normalized * realAttackRange;
        warningIndicator = WarningIndicator.Instantiate(aPoint, realAttackRange);
        warningIndicator.Play(2, (indi) =>
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(attackPoint.position, realAttackRange, LayerMask.GetMask("PlayerSide"));
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
        // meleeAttackIndicator.transform.right = Player.Instance.transform.position - transform.position;
        // meleeAttackIndicator.PlayIndicator(1.5f, () =>
        // {

        // });
    }

    public override void CancelAttack()
    {
        base.CancelAttack();
        EndAttack();
    }
    public override void EndAttack()
    {
        base.EndAttack();
        warningIndicator?.Cancel();
        warningIndicator = null;
    }
}