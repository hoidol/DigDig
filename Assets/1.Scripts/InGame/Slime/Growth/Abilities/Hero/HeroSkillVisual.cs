using System;
using UnityEngine;

// (2026-11-25) 용사 공격·스킬의 임시 그림 (프리팹 없이 실행 중 생성: 선·스프라이트)
// 이펙트가 정해지면 여기 대신 EffectManager 이펙트로 바꿀 것
public static class HeroSkillVisual
{
    public const string SORTING_LAYER = "OverDefault";
    public const int SORTING_ORDER = 50;

    static Material mat;
    public static Material Mat => mat != null ? mat : (mat = new Material(Shader.Find("Sprites/Default")) { name = "HeroSkillVisual", hideFlags = HideFlags.HideAndDontSave });

    public static LineRenderer CreateLine(string name, Color color, float width, bool loop = false, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.material = Mat;
        lr.useWorldSpace = true;
        lr.loop = loop;
        lr.startWidth = lr.endWidth = width;
        lr.startColor = lr.endColor = color;
        lr.numCapVertices = 3;
        lr.numCornerVertices = 2;
        lr.sortingLayerName = SORTING_LAYER;
        lr.sortingOrder = SORTING_ORDER;
        return lr;
    }

    public static void SetArc(LineRenderer lr, Vector2 center, float radius, float startDeg, float endDeg, int segments = 20)
    {
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.Lerp(startDeg, endDeg, i / (float)segments) * Mathf.Deg2Rad;
            lr.SetPosition(i, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius);
        }
    }

    public static void SetCircle(LineRenderer lr, Vector2 center, float radius, int segments = 40)
    {
        lr.loop = true;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            lr.SetPosition(i, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius);
        }
    }

    public static void SetAlpha(LineRenderer lr, Color baseColor, float alpha)
    {
        Color c = baseColor;
        c.a = baseColor.a * alpha;
        lr.startColor = lr.endColor = c;
    }

    public static SpriteRenderer CreateSprite(string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingLayerName = SORTING_LAYER;
        sr.sortingOrder = SORTING_ORDER + 1;
        return sr;
    }

    // 일정 시간 동안 매 프레임 update(0→1) 후 삭제
    public static HeroTimedVisual Run(GameObject go, float life, Action<float> update)
    {
        var v = go.AddComponent<HeroTimedVisual>();
        v.life = Mathf.Max(0.01f, life);
        v.update = update;
        update?.Invoke(0f);
        return v;
    }

    // 칼 휘두르기: 부채꼴 호가 dir 쪽으로 훑고 사라짐
    public static void PlaySwing(Vector2 center, Vector2 dir, float radius, float angle, Color color, float time = 0.18f)
    {
        float mid = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float from = mid + angle * 0.5f, to = mid - angle * 0.5f;
        var outer = CreateLine("HeroSwing", color, 0.22f);
        outer.widthCurve = new AnimationCurve(new Keyframe(0f, 0.02f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0.1f));
        Run(outer.gameObject, time, t =>
        {
            float sweep = Mathf.Clamp01(t * 1.6f);
            SetArc(outer, center, radius * 0.9f, from, Mathf.Lerp(from, to, sweep), 16);
            SetAlpha(outer, color, 1f - Mathf.Clamp01((t - 0.55f) / 0.45f));
        });
    }

    // (2026-11-26) 손톱 할퀴기: 휘두르기(PlaySwing)를 얇게 3줄 → 맞은 자리에 대각선으로 빠르게 그어지고 사라짐 (버서커 공격)
    // dir = 공격 방향, 줄은 dir을 CLAW_TILT도 기울인 방향, 가운데 줄이 조금 길고 줄마다 살짝 늦게 그어짐
    const float CLAW_TILT = 55f;
    public static void PlayClaw(Vector2 center, Vector2 dir, Color color, float length = 0.75f, float width = 0.07f, float time = 0.2f)
    {
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        Vector2 along = (Vector2)(Quaternion.Euler(0f, 0f, -CLAW_TILT) * dir.normalized);
        Vector2 side = new Vector2(-along.y, along.x);
        const float GAP = 0.15f;
        for (int k = -1; k <= 1; k++)
        {
            float len = length * (k == 0 ? 1f : 0.82f);
            Vector2 c = center + side * (GAP * k);
            Vector2 from = c - along * (len * 0.5f), to = c + along * (len * 0.5f);
            Vector2 bow = side * (len * 0.08f); // 살짝 휘게
            float delay = (k + 1) * 0.12f;      // 0 → 1 시간 중 줄마다 늦게 시작
            var lr = CreateLine("Claw", color, width);
            lr.numCapVertices = 0;
            lr.widthCurve = new AnimationCurve(new Keyframe(0f, 0.05f), new Keyframe(0.35f, 1f), new Keyframe(1f, 0f));
            lr.positionCount = 6;
            Run(lr.gameObject, time, t =>
            {
                float draw = Mathf.Clamp01((t - delay) / 0.4f);
                for (int i = 0; i < 6; i++)
                {
                    float u = draw * i / 5f;
                    lr.SetPosition(i, Vector2.Lerp(from, to, u) + bow * Mathf.Sin(u * Mathf.PI));
                }
                SetAlpha(lr, color, draw <= 0f ? 0f : 1f - Mathf.Clamp01((t - 0.6f) / 0.4f));
            });
        }
    }

    // 착탄 경고 원 → 사라짐
    public static void PlayRing(Vector2 center, float radius, Color color, float time = 0.35f, float width = 0.08f)
    {
        var ring = CreateLine("HeroRing", color, width, true);
        Run(ring.gameObject, time, t =>
        {
            SetCircle(ring, center, radius * Mathf.Lerp(0.6f, 1.05f, t));
            SetAlpha(ring, color, 1f - t);
        });
    }
}

// 정해진 시간 동안 콜백을 부르고 스스로 삭제 (게임 일시정지 중엔 멈춤 = Time.deltaTime)
public class HeroTimedVisual : MonoBehaviour
{
    public float life;
    public Action<float> update;
    float t;

    void Update()
    {
        t += Time.deltaTime;
        update?.Invoke(Mathf.Clamp01(t / life));
        if (t >= life)
            Destroy(gameObject);
    }
}
