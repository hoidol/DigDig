using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class InGameUtil
{
    //범위 공격
    public static void DamageEnemies(Vector2 center, float radius, float damage, AttackType attackType, LayerMask enemyLayer, int maxCount = 4)
    {
        int count = maxCount;
        Collider2D[] cols = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        foreach (var col in cols)
        {
            if (col.TryGetComponent(out IHittable h))
            {
                h.TakeDamage(new DamageData() { damage = damage, attackType = attackType });
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
        return FindTarget(pos, range, layerMask, FindTargetType.Closest);
    }

    //exceptEffect 이펙트가 적용된 타겟을 후순위로 찾음
    public static Transform FindTarget(Vector2 pos, float range, LayerMask layerMask, string exceptEffect)
    {
        return FindTarget(pos, range, layerMask, FindTargetType.Closest, exceptEffect);
    }

    //범위 내 적을 type 기준으로 찾기, exceptEffect가 있으면 해당 이펙트가 적용된 타겟은 후순위
    public static Transform FindTarget(Vector2 pos, float range, LayerMask layerMask, FindTargetType type, string exceptEffect = null)
    {
        List<Transform> targets = FindTargets(pos, range, layerMask, type, 1, exceptEffect);
        return targets.Count > 0 ? targets[0] : null;
    }

    //범위 내 서로 다른 적 최대 count개를 type 우선순위 순으로 찾기, exceptEffect가 적용된 타겟은 후순위
    public static List<Transform> FindTargets(Vector2 pos, float range, LayerMask layerMask, FindTargetType type, int count, string exceptEffect = null)
    {
        List<Transform> result = new List<Transform>(count);
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, range, layerMask);
        if (cols.Length == 0 || count <= 0)
            return result;

        List<Collider2D> normals = new List<Collider2D>(cols.Length);
        List<Collider2D> effectings = new List<Collider2D>();
        for (int i = 0; i < cols.Length; i++)
        {
            bool isEffecting = !string.IsNullOrEmpty(exceptEffect) && cols[i].TryGetComponent(out StatusEffectHandler handler) && handler.HasEffect(exceptEffect);
            if (isEffecting)
                effectings.Add(cols[i]);
            else
                normals.Add(cols[i]);
        }

        PickTargets(normals, pos, type, count, result);
        PickTargets(effectings, pos, type, count, result);
        return result;
    }

    // SelectTarget으로 하나씩 뽑고 후보에서 제거 → 중복 없이 우선순위 순
    static void PickTargets(List<Collider2D> candidates, Vector2 pos, FindTargetType type, int count, List<Transform> result)
    {
        while (result.Count < count && candidates.Count > 0)
        {
            Transform picked = SelectTarget(candidates, pos, type);
            if (picked == null)
                return;
            result.Add(picked);
            candidates.RemoveAll(c => c.transform == picked);
        }
    }

    static Transform SelectTarget(IList<Collider2D> cols, Vector2 pos, FindTargetType type)
    {
        if (cols.Count == 0)
            return null;

        switch (type)
        {
            case FindTargetType.Random:
                return cols[Random.Range(0, cols.Count)].transform;

            case FindTargetType.Farthest:
                {
                    Collider2D farthest = null;
                    float farthestSqrDist = float.MinValue;
                    for (int i = 0; i < cols.Count; i++)
                    {
                        float sqrDist = ((Vector2)cols[i].transform.position - pos).sqrMagnitude;
                        if (sqrDist > farthestSqrDist)
                        {
                            farthest = cols[i];
                            farthestSqrDist = sqrDist;
                        }
                    }
                    return farthest.transform;
                }

            case FindTargetType.LowHp:
                {
                    //체력이 같으면 가까운 적 우선
                    Collider2D lowest = null;
                    float lowestHp = float.MaxValue;
                    float lowestSqrDist = float.MaxValue;
                    for (int i = 0; i < cols.Count; i++)
                    {
                        if (!cols[i].TryGetComponent(out IHittable h))
                            continue;

                        float sqrDist = ((Vector2)cols[i].transform.position - pos).sqrMagnitude;
                        if (h.CurHp < lowestHp || (h.CurHp == lowestHp && sqrDist < lowestSqrDist))
                        {
                            lowest = cols[i];
                            lowestHp = h.CurHp;
                            lowestSqrDist = sqrDist;
                        }
                    }
                    //체력 정보가 없으면 가까운 적
                    return lowest != null ? lowest.transform : SelectTarget(cols, pos, FindTargetType.Closest);
                }


            case FindTargetType.HighHp:
                {
                    //체력이 같으면 가까운 적 우선
                    Collider2D highest = null;
                    float highestHp = float.MinValue;
                    float highestSqrDist = float.MaxValue;
                    for (int i = 0; i < cols.Count; i++)
                    {
                        if (!cols[i].TryGetComponent(out IHittable h))
                            continue;

                        float sqrDist = ((Vector2)cols[i].transform.position - pos).sqrMagnitude;
                        if (h.CurHp > highestHp || (h.CurHp == highestHp && sqrDist < highestSqrDist))
                        {
                            highest = cols[i];
                            highestHp = h.CurHp;
                            highestSqrDist = sqrDist;
                        }
                    }
                    //체력 정보가 없으면 가까운 적
                    return highest != null ? highest.transform : SelectTarget(cols, pos, FindTargetType.Closest);
                }

            case FindTargetType.Closest:
            default:
                {
                    Collider2D nearest = null;
                    float nearestSqrDist = float.MaxValue;
                    for (int i = 0; i < cols.Count; i++)
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
    }
    static float endOutLength = 0;
    public static float GetEndOutLength() // 화면 중심에서 모서리까지의 대각선 길이
    {
        if (endOutLength <= 0)
        {
            Camera cam = CameraManager.Instance.mainCamera;
            endOutLength = new Vector2(cam.orthographicSize, cam.orthographicSize * cam.aspect).magnitude + 0.5f;
        }

        return endOutLength;
    }


    public static Vector2 GetSpawnTopPosition(float howFar = -1)
    {
        if (howFar < 0)
        {              
            howFar = GetEndOutLength();
        }

        Vector2 direction = Quaternion.Euler(0, 0, UnityEngine.Random.Range(60f, 120f)) * Vector2.right;
        return MapManager.SnappedPosition(Vector2.zero + direction * howFar);
    }
}
public enum FindTargetType
{
    Closest, //가까운
    Farthest, //먼
    Random, //랜덤 
    LowHp,
    HighHp,
    //높은 체력

    Count
}
