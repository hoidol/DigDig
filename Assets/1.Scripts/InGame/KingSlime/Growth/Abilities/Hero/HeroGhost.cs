using System.Collections.Generic;
using UnityEngine;

// (2026-11-23) 네크로멘서 발동 스킬의 망령: 적 주 경로(EnemyFieldController.MainRoute)를 끝 → 처음으로 거꾸로 따라가고, 처음에 닿으면 다시 끝에서
// 닿은 적(radius 안)에게 같은 적은 hitInterval초마다 피해. duration초 뒤 사라짐
// 그림 = 킹 몸 스프라이트를 반투명 보라색으로 (프리팹 없이 실행 중 생성)
public class HeroGhost : MonoBehaviour
{
    NecromancerHero owner;
    float timer, speed, radius, hitInterval;
    int targetIdx;
    readonly Dictionary<Enemy, float> lastHit = new Dictionary<Enemy, float>();
    public static int ActiveCount { get; private set; }

    public static HeroGhost Spawn(KingSlime king, NecromancerHero owner, float duration, float speed, float radius, float hitInterval)
    {
        var paths = EnemyFieldController.Instance != null ? EnemyFieldController.Instance.MainRoute : null;
        if (paths == null || paths.Length == 0)
            return null;
        var go = new GameObject("HeroGhost");
        var ghost = go.AddComponent<HeroGhost>();
        ghost.owner = owner;
        ghost.timer = duration;
        ghost.speed = speed;
        ghost.radius = radius;
        ghost.hitInterval = hitInterval;
        go.transform.position = paths[paths.Length - 1].position;
        ghost.targetIdx = paths.Length - 2;

        SpriteRenderer src = null;
        foreach (var r in king.GetComponentsInChildren<SpriteRenderer>(true))
            if (r.sprite != null) { src = r; break; }
        var sr = go.AddComponent<SpriteRenderer>();
        if (src != null)
        {
            sr.sprite = src.sprite;
            sr.sortingLayerID = src.sortingLayerID;
            sr.sortingOrder = src.sortingOrder + 10;
            go.transform.localScale = src.transform.lossyScale;
        }
        sr.color = new Color(0.7f, 0.45f, 1f, 0.65f);
        ActiveCount++;
        return ghost;
    }

    void OnDestroy() => ActiveCount--;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;
        timer -= Time.deltaTime;
        if (timer <= 0f || owner == null)
        {
            Destroy(gameObject);
            return;
        }
        Move();
        HitEnemies();
    }

    void Move()
    {
        var paths = EnemyFieldController.Instance != null ? EnemyFieldController.Instance.MainRoute : null;
        if (paths == null || paths.Length == 0)
            return;
        if (targetIdx < 0)
        {
            transform.position = paths[paths.Length - 1].position; // 처음에 닿으면 다시 끝에서
            targetIdx = paths.Length - 2;
            if (targetIdx < 0)
                return;
        }
        Vector2 target = paths[targetIdx].position;
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (((Vector2)transform.position - target).sqrMagnitude < 0.0001f)
            targetIdx--;
    }

    void HitEnemies()
    {
        if (EnemyFieldController.Instance == null)
            return;
        float sqr = radius * radius;
        var hits = new List<Enemy>();
        foreach (Enemy e in EnemyFieldController.Instance.GetTargetEnemies()) // (2026-11-25) 보스 포함
        {
            if (((Vector2)(e.transform.position - transform.position)).sqrMagnitude > sqr)
                continue;
            if (lastHit.TryGetValue(e, out float t) && Time.time - t < hitInterval)
                continue;
            hits.Add(e);
        }
        foreach (Enemy e in hits)
        {
            lastHit[e] = Time.time;
            owner.GhostHit(e);
        }
    }
}
