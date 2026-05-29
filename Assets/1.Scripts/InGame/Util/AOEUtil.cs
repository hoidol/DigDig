using UnityEngine;

public static class AOEUtil
{
    public static void DamageEnemies(Vector2 center, float radius, float damage, LayerMask enemyLayer, int maxCount = 4)
    {
        int count = maxCount;
        Collider2D[] cols = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        foreach (var col in cols)
        {
            if (col.TryGetComponent(out IHittable h))
            {
                h.TakeDamage(new DamageData() { damage = damage });
                if (maxCount > 0)
                {
                    count--;
                    if (count <= 0)
                        break;
                }
            }

        }
    }
}
