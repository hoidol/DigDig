using UnityEngine;

// (2026-11-25) 둔화 표시: 적 발밑에 납작한 노란색 고리가 천천히 출렁임 (얼음 표시 X)
// 적 프리팹(StatusEffect 하위)에 effectKey "Slow"인 표시가 없으면 StatusEffectHandler.Init이 실행 중에 이걸 붙임
// → 프리팹에 "Slow" 표시를 직접 만들면 그쪽을 씀 (모양 교체는 프리팹에서)
public class SlowFeetView : StatusEffectView
{
    public static readonly Color COLOR = new Color(1f, 0.85f, 0.25f, 0.9f);
    const int SEGMENTS = 32;
    const float RADIUS = 0.42f;
    const float FLATTEN = 0.38f;

    LineRenderer outer, inner;
    float t;

    public static SlowFeetView Create(Transform enemyRoot)
    {
        var go = new GameObject("SlowFeetView");
        go.transform.SetParent(enemyRoot, false);
        var view = go.AddComponent<SlowFeetView>();
        view.effectKey = SlowEffect.EFFECT_KEY;
        view.outer = NewRing("Outer", go.transform, 0.07f);
        view.inner = NewRing("Inner", go.transform, 0.04f);
        view.PlaceAtFeet(enemyRoot);
        go.SetActive(false);
        return view;
    }

    static LineRenderer NewRing(string name, Transform parent, float width)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial = HeroSkillVisual.Mat;
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = SEGMENTS;
        lr.widthMultiplier = width;
        lr.startColor = lr.endColor = COLOR;
        lr.numCapVertices = 2;
        return lr;
    }

    // 발밑 = 몸 그림 아래 끝, 정렬은 몸 바로 뒤
    void PlaceAtFeet(Transform enemyRoot)
    {
        SpriteRenderer body = null;
        foreach (var sr in enemyRoot.GetComponentsInChildren<SpriteRenderer>(true))
            if (sr.sprite != null && (body == null || sr.bounds.size.sqrMagnitude > body.bounds.size.sqrMagnitude))
                body = sr;
        float y = body != null ? body.bounds.min.y - enemyRoot.position.y + 0.08f : -0.3f;
        transform.localPosition = new Vector3(0f, y / Mathf.Max(0.0001f, enemyRoot.lossyScale.y), 0f);
        foreach (var lr in new[] { outer, inner })
        {
            if (body != null)
            {
                lr.sortingLayerID = body.sortingLayerID;
                lr.sortingOrder = body.sortingOrder - 1;
            }
        }
    }

    public override void Play()
    {
        base.Play();
        t = 0f;
    }

    void Update()
    {
        t += Time.deltaTime;
        float pulse = 1f + Mathf.Sin(t * 5f) * 0.08f;
        SetEllipse(outer, RADIUS * pulse);
        SetEllipse(inner, RADIUS * 0.6f * (2f - pulse));
        Color c = COLOR;
        c.a = COLOR.a * (0.7f + 0.3f * Mathf.Sin(t * 5f + 1f));
        outer.startColor = outer.endColor = c;
        c.a *= 0.6f;
        inner.startColor = inner.endColor = c;
    }

    static void SetEllipse(LineRenderer lr, float radius)
    {
        for (int i = 0; i < SEGMENTS; i++)
        {
            float a = i * Mathf.PI * 2f / SEGMENTS;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius * FLATTEN, 0f));
        }
    }
}
