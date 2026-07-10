using UnityEngine;

public static class InGameUtil
{
    //범위 공격
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

    //백어택 
    public static bool CheckBackAttack(Transform target, int face, Vector2 targetPoint)
    {
        float x = targetPoint.x - target.position.x ; //x 0보면 크면 오론쪽, 작으면 왼쪽 
        if ((face > 0 && x <0) || (face < 0 && x > 0)) 
        {
            return true;
        }
        return false;
    }
}
