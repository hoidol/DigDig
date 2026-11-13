using UnityEngine;

// 지점 배열을 순서대로 이은 닫힌 고리(마지막 → 첫 지점까지) 계산. 경로를 도는 이동 오브젝트용 (HailMinion 등)
public static class PathLoop
{
    // 고리 전체 길이 (지점이 2개 미만이면 0)
    public static float GetLength(Transform[] points)
    {
        if (points == null || points.Length < 2)
            return 0f;
        float total = 0f;
        for (int i = 0; i < points.Length; i++)
            total += Vector2.Distance(points[i].position, points[(i + 1) % points.Length].position);
        return total;
    }

    // 지점 순서대로 돌았을 때 시계 방향인지 (신발끈 공식 부호 넓이, y가 위쪽인 월드 기준. 양수 = 반시계)
    public static bool IsClockwise(Transform[] points)
    {
        if (points == null || points.Length < 3)
            return true; // 2개 이하는 방향 개념 없음 → 지점 순서 그대로
        float area = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 a = points[i].position;
            Vector2 b = points[(i + 1) % points.Length].position;
            area += a.x * b.y - b.x * a.y;
        }
        return area < 0f;
    }

    // 원하는 회전 방향으로 가려면 진행 거리를 더할지(+1) 뺄지(-1). 지점 순서 방향과 같으면 +1
    public static float GetTravelSign(Transform[] points, RotationDirection direction)
    {
        bool wantClockwise = direction == RotationDirection.Clockwise;
        return IsClockwise(points) == wantClockwise ? 1f : -1f;
    }

    // 고리 위에서 pos와 가장 가까운 점 (지점이 없으면 pos 그대로)
    public static Vector2 GetNearestPoint(Transform[] points, Vector2 pos)
    {
        if (points == null || points.Length == 0)
            return pos;
        if (points.Length == 1)
            return points[0].position;

        Vector2 best = points[0].position;
        float minDist = float.MaxValue;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 a = points[i].position;
            Vector2 b = points[(i + 1) % points.Length].position;
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(pos - a, ab) / ab.sqrMagnitude) : 0f;
            Vector2 p = a + ab * t;
            float dist = (p - pos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                best = p;
            }
        }
        return best;
    }

    // 첫 지점부터 distance만큼 간 위치 (total = GetLength, distance는 total을 넘으면 반복)
    public static Vector2 GetPoint(Transform[] points, float total, float distance)
    {
        float d = Mathf.Repeat(distance, total);
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 a = points[i].position;
            Vector2 b = points[(i + 1) % points.Length].position;
            float len = Vector2.Distance(a, b);
            if (d <= len && len > 0f)
                return Vector2.Lerp(a, b, d / len);
            d -= len;
        }
        return points[0].position;
    }
}

// 경로 회전 방향 (지점 배열 순서와 상관없이 화면 기준)
public enum RotationDirection
{
    Clockwise,         // 시계 방향
    CounterClockwise,  // 반시계 방향
}
