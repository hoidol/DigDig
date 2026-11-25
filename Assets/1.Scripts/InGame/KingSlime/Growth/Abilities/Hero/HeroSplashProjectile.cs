using UnityEngine;

// (2026-11-25) 사령술사 기본 공격 "카오스 폭발탄": 타겟 쪽으로 날아가 적에게 닿거나 목표 지점에 도착하면 폭발
//   → 반경 radius 안 적 전부 피해 (NecromancerHero.SplashHit, 킹 칸 룬 적중 효과 포함). 그림 = 보라 구슬 + 폭발 고리 (임시)
// (이전 원거리 슬래쉬 HeroSlashProjectile 대체, 같은 파일 guid)
public class HeroSplashProjectile : MonoBehaviour
{
    NecromancerHero owner;
    Transform target;
    Vector2 dir, targetPos;
    float speed, remain, radius;
    Color color;
    LineRenderer orb;
    public static int ActiveCount { get; private set; }
    const float HIT_DISTANCE = 0.35f;

    public static HeroSplashProjectile Spawn(NecromancerHero owner, Vector2 pos, Transform target, Vector2 dir, float speed, float maxDistance, float radius, Color color)
    {
        var go = new GameObject("HeroSplash");
        go.transform.position = pos;
        var p = go.AddComponent<HeroSplashProjectile>();
        p.owner = owner;
        p.target = target;
        p.dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        p.targetPos = target != null ? (Vector2)target.position : pos + p.dir * maxDistance;
        p.speed = speed;
        p.remain = maxDistance;
        p.radius = radius;
        p.color = color;
        p.orb = HeroSkillVisual.CreateLine("Orb", color, 0.18f, true, go.transform);
        HeroSkillVisual.SetCircle(p.orb, pos, 0.12f, 12);
        ActiveCount++;
        return p;
    }

    void OnDestroy() => ActiveCount--;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }
        // 타겟이 살아 있으면 따라감, 죽으면 마지막 위치로
        if (target != null && target.gameObject.activeInHierarchy && target.TryGetComponent(out Enemy e) && e.CurHp > 0)
            targetPos = target.position;
        Vector2 pos = transform.position;
        Vector2 to = targetPos - pos;
        float step = Mathf.Min(speed * Time.deltaTime, remain);
        if (to.magnitude <= Mathf.Max(HIT_DISTANCE, step) || remain <= 0f)
        {
            Explode(to.magnitude <= Mathf.Max(HIT_DISTANCE, step) ? targetPos : pos);
            return;
        }
        dir = to.normalized;
        pos += dir * step;
        remain -= step;
        transform.position = pos;
        HeroSkillVisual.SetCircle(orb, pos, 0.12f, 12);
        if (HitsAnyEnemy(pos))
            Explode(pos);
    }

    bool HitsAnyEnemy(Vector2 pos)
    {
        if (EnemyFieldController.Instance == null)
            return false;
        foreach (Enemy e in EnemyFieldController.Instance.GetTargetEnemies())
            if (e != null && e.CurHp > 0 && ((Vector2)e.transform.position - pos).sqrMagnitude <= HIT_DISTANCE * HIT_DISTANCE)
                return true;
        return false;
    }

    void Explode(Vector2 pos)
    {
        owner.SplashExplode(pos, radius, dir);
        Destroy(gameObject);
    }
}
