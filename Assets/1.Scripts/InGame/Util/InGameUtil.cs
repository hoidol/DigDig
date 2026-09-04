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
        float x = targetPoint.x - target.position.x; //x 0보면 크면 오론쪽, 작으면 왼쪽 
        if ((face > 0 && x < 0) || (face < 0 && x > 0))
        {
            return true;
        }
        return false;
    }

    //범위 내 가장 가까운 적 찾기
    public static Transform FindTarget(Vector2 pos, float range, LayerMask layerMask)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, range, layerMask);
        if (cols.Length == 0)
            return null;

        Collider2D nearest = cols[0];
        float nearestSqrDist = ((Vector2)nearest.transform.position - pos).sqrMagnitude;
        for (int i = 1; i < cols.Length; i++)
        {
            float sqrDist = ((Vector2)cols[i].transform.position - pos).sqrMagnitude;
            if (sqrDist < nearestSqrDist)
            {
                nearest = cols[i];
                nearestSqrDist = sqrDist;
            }
        }

        return nearest.transform;
    }
}
public enum FindTargetType
{
    Closest, //현재 기준 가장 가까운 적
    ClosestToCharacter, //현재 기준 가장 가까운 적
    LowHp,
}
