using UnityEngine;
public class MeleeEnemy : NormalEnemy
{
    MeleeAttackIndicator meleeAttackIndicator;
    public Transform attackPoint;
    protected override void Awake()
    {
        base.Awake();
        meleeAttackIndicator = GetComponentInChildren<MeleeAttackIndicator>();
    }
    DamageData damageData = new DamageData();
    public override void Spawn(Vector2 pos)
    {
        base.Spawn(pos);
        meleeAttackIndicator.gameObject.SetActive(false);
        damageData.damage = enemyData.GetAttackPower();
    }
    protected override void StartAttack()
    {
        base.StartAttack();
        meleeAttackIndicator.transform.right = Player.Instance.transform.position - transform.position;
        meleeAttackIndicator.PlayIndicator(() =>
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(attackPoint.position, 1, LayerMask.GetMask("PlayerSide"));
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