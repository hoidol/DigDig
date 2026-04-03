using UnityEngine;

public static class AOEUtil
{
    public static void DamageEnemies(Vector2 center, float radius, float damage, LayerMask enemyLayer)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        foreach (var col in cols)
        {
            if (col.TryGetComponent(out IHittable h))
                h.TakeDamage(damage);
        }
    }
}
