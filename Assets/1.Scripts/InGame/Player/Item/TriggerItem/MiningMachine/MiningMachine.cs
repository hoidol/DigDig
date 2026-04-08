using UnityEngine;

public abstract class MiningMachine : MonoBehaviour
{
    public float attackRange    = 8f;
    public float attackPower    = 10f;
    public float attackInterval = 1f;
    public LayerMask oreLayer;

    protected float attackTimer;
    protected OreStone targetOre;

    protected virtual void Update()
    {
        FindTarget();

        if (targetOre == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0;
            Attack(targetOre);
        }
    }

    void FindTarget()
    {
        // 현재 타겟이 유효하면 유지
        if (targetOre != null && targetOre.gameObject.activeSelf)
        {
            float dist = Vector2.Distance(transform.position, targetOre.transform.position);
            if (dist <= attackRange) return;
        }

        targetOre = null;

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, attackRange, oreLayer);
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            if (!col.TryGetComponent(out OreStone ore)) continue;
            float dist = Vector2.Distance(transform.position, ore.transform.position);
            if (dist < minDist)
            {
                minDist  = dist;
                targetOre = ore;
            }
        }
    }

    protected abstract void Attack(OreStone ore);
}
