using UnityEngine;
public class MeleeEnemy : NormalEnemy
{
    // MeleeAttackIndicator meleeAttackIndicator;
    // public Transform attackPoint;
    public float realAttackRange = 1.5f;
    public WarningIndicator warningIndicator;
    public override void Awake()
    {
        base.Awake();
        // meleeAttackIndicator = GetComponentInChildren<MeleeAttackIndicator>(true);
    }
    public override void StartAttack()
    {
        base.StartAttack();
        Vector2 aPoint = transform.position + (Character.Instance.transform.position - transform.position).normalized * realAttackRange;
        warningIndicator = WarningIndicator.Instantiate(aPoint, realAttackRange);
        warningIndicator.transform.parent = transform;
        warningIndicator.Play(1, (indi) =>
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(aPoint, realAttackRange, LayerMask.GetMask("AllyUnit"));
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
        EndAttack();
    }
    public override void EndAttack()
    {
        base.EndAttack();
        warningIndicator?.Cancel();
        warningIndicator = null;
    }
}