using UnityEngine;

public abstract class MiningMachine : MonoBehaviour
{
    public float attackRange    = 8f;
    public float attackPower    = 10f;
    public float attackInterval = 1f;
    public LayerMask oreLayer;

    protected float attackTimer;
    protected Stone targetStone;

    protected virtual void Update()
    {
        FindTarget();

        if (targetStone == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0;
            Attack(targetStone);
        }
    }

    void FindTarget()
    {
        // 현재 타겟이 유효하면 유지
        if (targetStone != null && targetStone.gameObject.activeSelf)
        {
            float dist = Vector2.Distance(transform.position, targetStone.transform.position);
            if (dist <= attackRange) return;
        }

        targetStone = null;

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, attackRange, oreLayer);
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            if (!col.TryGetComponent(out Stone ore)) continue;
            float dist = Vector2.Distance(transform.position, ore.transform.position);
            if (dist < minDist)
            {
                minDist  = dist;
                targetStone = ore;
            }
        }
    }

    protected abstract void Attack(Stone ore);
}
