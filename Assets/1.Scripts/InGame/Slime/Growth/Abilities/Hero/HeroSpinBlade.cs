using System.Collections.Generic;
using UnityEngine;

// (2026-11-25) 기사 1단계 "칼날 회오리": 슬라임을 따라다니며 칼이 빠르게 돌고, 반경 안 적에게 같은 적은 hitInterval초마다 피해 + 둔화
// 그림 = 칼 스프라이트(KnightHero.swordSprite, 없으면 선) + 도는 원. 게임이 멈추면(isPlaying false) 같이 멈춤
public class HeroSpinBlade : MonoBehaviour
{
    KnightHero owner;
    float timer, radius, hitInterval, rotateSpeed, angle;
    readonly Dictionary<Enemy, float> lastHit = new Dictionary<Enemy, float>();
    Transform blade;
    LineRenderer ring;
    static readonly Color RING_COLOR = new Color(1f, 0.95f, 0.75f, 0.45f);

    public static HeroSpinBlade Spawn(KnightHero owner, float duration, float radius, float hitInterval, float rotateSpeed, Sprite sword)
    {
        var go = new GameObject("HeroSpinBlade");
        var s = go.AddComponent<HeroSpinBlade>();
        s.owner = owner;
        s.timer = duration;
        s.radius = radius;
        s.hitInterval = hitInterval;
        s.rotateSpeed = rotateSpeed;
        go.transform.position = owner.Slime.transform.position;

        // 칼 2자루 (반대편) = 빠르게 도는 느낌
        var bladeGo = new GameObject("Blades");
        bladeGo.transform.SetParent(go.transform, false);
        s.blade = bladeGo.transform;
        for (int i = 0; i < 2; i++)
        {
            float a = i * 180f;
            Vector2 offset = Quaternion.Euler(0, 0, a) * Vector2.right * radius * 0.6f;
            if (sword != null)
            {
                SpriteRenderer sr = HeroSkillVisual.CreateSprite("Sword", sword, Color.white);
                sr.transform.SetParent(bladeGo.transform, false);
                sr.transform.localPosition = offset;
                sr.transform.localRotation = Quaternion.Euler(0, 0, a); // 칼끝이 바깥을 보게 (그림은 칼끝이 오른쪽)
                float size = Mathf.Max(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
                sr.transform.localScale = Vector3.one * (radius * 0.9f / Mathf.Max(0.01f, size));
            }
            else
            {
                LineRenderer lr = HeroSkillVisual.CreateLine("Sword", Color.white, 0.12f, false, bladeGo.transform);
                lr.useWorldSpace = false;
                lr.positionCount = 2;
                lr.SetPosition(0, offset * 0.3f);
                lr.SetPosition(1, offset * 1.6f);
            }
        }
        s.ring = HeroSkillVisual.CreateLine("Ring", RING_COLOR, 0.06f, true, go.transform);
        return s;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;
        timer -= Time.deltaTime;
        if (timer <= 0f || owner == null || owner.Slime == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector2 center = owner.Slime.transform.position;
        transform.position = center;
        angle -= rotateSpeed * Time.deltaTime;
        blade.localRotation = Quaternion.Euler(0, 0, angle);
        HeroSkillVisual.SetCircle(ring, center, radius);
        HeroSkillVisual.SetAlpha(ring, RING_COLOR, timer < 0.5f ? timer / 0.5f : 1f);
        HitEnemies(center);
    }

    void HitEnemies(Vector2 center)
    {
        if (EnemyFieldController.Instance == null)
            return;
        float sqr = radius * radius;
        var hits = new List<Enemy>();
        foreach (Enemy e in EnemyFieldController.Instance.GetTargetEnemies())
        {
            if (((Vector2)e.transform.position - center).sqrMagnitude > sqr)
                continue;
            if (lastHit.TryGetValue(e, out float t) && Time.time - t < hitInterval)
                continue;
            hits.Add(e);
        }
        foreach (Enemy e in hits)
        {
            lastHit[e] = Time.time;
            owner.SpinHit(e);
        }
    }
}
