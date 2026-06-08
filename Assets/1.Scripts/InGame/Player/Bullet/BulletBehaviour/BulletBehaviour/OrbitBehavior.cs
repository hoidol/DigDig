using UnityEngine;

// 적중 시 고정 반경으로 1회전 공전 후 다음 타격 반복. 마지막 공전 후 소멸.
public class OrbitBehavior : IBulletBehavior
{
    int remaining;
    readonly float radius;
    readonly float angularSpeed; // degrees/sec

    bool isOrbiting;
    Vector2 center;
    float currentAngleDeg;
    float orbitTimer;
    float orbitDuration; // 1회전에 걸리는 시간

    public OrbitBehavior(int bounceCount, float radius = 1.5f, float angularSpeed = 360f)
    {
        remaining = bounceCount;
        this.radius = radius;
        this.angularSpeed = angularSpeed;
        orbitDuration = 360f / angularSpeed;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D)
    {
        remaining--;
        center = hit2D.point;
        currentAngleDeg = Vector2.SignedAngle(Vector2.right,
            (Vector2)bullet.transform.position - center);
        isOrbiting = true;
        orbitTimer = 0f;
        return false; // 공전 중엔 소멸 안 함
    }

    public void OnMove(BulletObject bullet)
    {
        if (!isOrbiting) return;

        bullet.skipDefaultMove = true;
        orbitTimer += Time.deltaTime;

        if (orbitTimer >= orbitDuration)
        {
            isOrbiting = false;
            bullet.skipDefaultMove = false;
            if (remaining <= 0) { bullet.Release(); return; }
            return;
        }

        currentAngleDeg += angularSpeed * Time.deltaTime;
        float rad = currentAngleDeg * Mathf.Deg2Rad;
        bullet.transform.position = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

        // 탄이 공전 접선 방향을 향하도록
        float tangentRad = rad + Mathf.PI * 0.5f;
        bullet.SetDirection(new Vector2(Mathf.Cos(tangentRad), Mathf.Sin(tangentRad)));
    }

    public void Merge(IBulletBehavior other)
    {
        remaining += ((OrbitBehavior)other).remaining;
    }
}
