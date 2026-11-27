using UnityEngine;

// (2026-11-25) 레인저 1단계 "화살비": 누른 곳 원 안에 duration초 동안 화살이 쏟아지고, tickInterval초마다 범위 피해 (발동 즉시 1번)
// 그림 = 바닥 원 + 비스듬히 떨어지는 짧은 선 (임시)
public class HeroArrowRain : MonoBehaviour
{
    static readonly Color RING_COLOR = new Color(0.45f, 1f, 0.55f, 0.6f);
    static readonly Color ARROW_COLOR = new Color(0.95f, 1f, 0.85f, 0.95f);
    const float ARROW_SPAWN_INTERVAL = 0.05f;
    const float ARROW_FALL_TIME = 0.18f;

    RangerHero owner;
    Vector2 center;
    float radius, timer, duration, tickInterval, tickTimer, arrowTimer;
    int ticksLeft;
    LineRenderer ring;

    public static HeroArrowRain Spawn(RangerHero owner, Vector2 pos, float radius, float duration, float tickInterval)
    {
        var go = new GameObject("HeroArrowRain");
        go.transform.position = pos;
        var r = go.AddComponent<HeroArrowRain>();
        r.owner = owner;
        r.center = pos;
        r.radius = radius;
        r.duration = duration;
        r.timer = duration;
        r.tickInterval = Mathf.Max(0.05f, tickInterval);
        r.ticksLeft = Mathf.Max(1, Mathf.RoundToInt(duration / r.tickInterval));
        r.ring = HeroSkillVisual.CreateLine("Ring", RING_COLOR, 0.07f, true, go.transform);
        HeroSkillVisual.SetCircle(r.ring, pos, radius);
        r.Tick(); // 발동 즉시 1번
        return r;
    }

    void Tick()
    {
        if (ticksLeft <= 0)
            return;
        ticksLeft--;
        tickTimer = tickInterval;
        if (owner != null)
            owner.RainTick(center);
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;
        float dt = Time.deltaTime;
        timer -= dt;
        if (owner == null || (timer <= 0f && ticksLeft <= 0))
        {
            Destroy(gameObject);
            return;
        }
        tickTimer -= dt;
        if (tickTimer <= 0f)
            Tick();

        HeroSkillVisual.SetAlpha(ring, RING_COLOR, timer < 0.4f ? Mathf.Max(0f, timer) / 0.4f : 1f);
        if (timer <= 0f)
            return;
        arrowTimer -= dt;
        while (arrowTimer <= 0f)
        {
            arrowTimer += ARROW_SPAWN_INTERVAL;
            SpawnArrow();
        }
    }

    // 원 안 랜덤 지점으로 위에서 비스듬히 떨어지는 짧은 선
    void SpawnArrow()
    {
        Vector2 hit = center + Random.insideUnitCircle * radius;
        Vector2 from = hit + new Vector2(-0.6f, 2.2f);
        LineRenderer lr = HeroSkillVisual.CreateLine("Arrow", ARROW_COLOR, 0.05f);
        lr.widthCurve = new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(1f, 1f));
        lr.positionCount = 2;
        HeroSkillVisual.Run(lr.gameObject, ARROW_FALL_TIME, t =>
        {
            Vector2 head = Vector2.Lerp(from, hit, t);
            Vector2 tail = head + (from - hit).normalized * 0.45f;
            lr.SetPosition(0, tail);
            lr.SetPosition(1, head);
        });
    }
}
