using System.Collections.Generic;
using UnityEngine;

// 찌릿이 (2026-11-26): 진화한 찌릿이가 필드(타일 위)의 다른 찌릿이 모두와 번개 선으로 연결 (예전 광선 미니언 방식)
// 선에 닿은 적 = tickInterval초마다 공격력 × lineDamageMul 피해 + 감속. 진화한 찌릿이끼리는 한 선만 (인스턴스 번호가 작은 쪽이 그림)
// 혼자면 선 없음 (기본 공격은 그대로). 수치 = MinionEvolutionSetting.lightning
public class LightningEvolution : MinionEvolution
{
    LightningEvolutionConfig Cfg => Config<LightningEvolutionConfig>();
    readonly List<LineRenderer> lines = new List<LineRenderer>();
    readonly List<Minion> partners = new List<Minion>();
    readonly HashSet<IHittable> hitThisTick = new HashSet<IHittable>();
    float tickTimer;
    const int LINE_POINTS = 8;
    const int MAX_SPARKS_PER_TICK = 4; // 적이 많을 때 이펙트가 쌓이지 않게

    void Update()
    {
        LightningEvolutionConfig cfg = Cfg;
        if (cfg == null || minion == null)
            return;
        FindPartners();
        bool show = !minion.IsBeingDragged && TileManager.Instance != null && TileManager.Instance.GetTile(minion) != null;
        EnsureLines(show ? partners.Count : 0, cfg);
        if (!show)
            return;
        Vector2 from = minion.transform.position;
        for (int i = 0; i < partners.Count; i++)
            DrawBolt(lines[i], from, partners[i].transform.position, cfg);

        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
            return;
        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f || partners.Count == 0)
            return;
        tickTimer = Mathf.Max(0.05f, cfg.tickInterval);
        hitThisTick.Clear();
        float damage = minion.AttackPower * cfg.lineDamageMul;
        foreach (Minion p in partners)
        {
            Vector2 to = p.transform.position;
            Vector2 d = to - from;
            float dist = d.magnitude;
            if (dist < 0.01f)
                continue;
            foreach (RaycastHit2D h in Physics2D.CircleCastAll(from, cfg.lineWidth * 0.5f, d / dist, dist, minion.targetLayerMask))
            {
                if (!h.collider.TryGetComponent(out IHittable e) || !hitThisTick.Add(e))
                    continue;
                DealDamage(minion, e, damage, AttackType.Magic);
                if (hitThisTick.Count <= MAX_SPARKS_PER_TICK && EffectManager.Instance != null)
                    EffectManager.Instance.Play(EffectType.Hit, e.Transform.position); // (2026-11-26) 맞은 적 스파크
                if (cfg.slowDuration > 0f)
                    ApplyStatus(e, new SlowEffect(cfg.slowDuration));
            }
        }
    }

    // 필드의 다른 찌릿이 (드래그 중·타일 밖 제외). 둘 다 진화했으면 인스턴스 번호가 작은 쪽만 연결
    void FindPartners()
    {
        partners.Clear();
        MinionFieldController field = MinionFieldController.Instance;
        if (field == null || TileManager.Instance == null)
            return;
        foreach (Minion m in field.activeminions)
        {
            if (m == null || m == minion || !m.gameObject.activeInHierarchy || m.key != minion.key || m.IsBeingDragged)
                continue;
            if (TileManager.Instance.GetTile(m) == null)
                continue;
            if (m.EvolutionAbility is LightningEvolution && m.GetInstanceID() < minion.GetInstanceID())
                continue;
            partners.Add(m);
        }
    }

    void EnsureLines(int count, LightningEvolutionConfig cfg)
    {
        while (lines.Count < count)
        {
            LineRenderer lr = HeroSkillVisual.CreateLine("LightningLink", cfg.lineColor, 0.1f, false, transform);
            lr.positionCount = LINE_POINTS;
            lines.Add(lr);
        }
        for (int i = 0; i < lines.Count; i++)
            if (lines[i] != null && lines[i].gameObject.activeSelf != (i < count))
                lines[i].gameObject.SetActive(i < count);
    }

    // 지글지글한 번개 선 (가운데 점들을 매 프레임 조금씩 흔듦)
    static void DrawBolt(LineRenderer lr, Vector2 from, Vector2 to, LightningEvolutionConfig cfg)
    {
        Vector2 d = to - from;
        Vector2 side = d.sqrMagnitude > 0.0001f ? new Vector2(-d.y, d.x).normalized : Vector2.zero;
        lr.startColor = lr.endColor = cfg.lineColor;
        lr.startWidth = lr.endWidth = Mathf.Max(0.06f, cfg.lineWidth * 0.4f);
        for (int i = 0; i < LINE_POINTS; i++)
        {
            float t = i / (float)(LINE_POINTS - 1);
            float jitter = i == 0 || i == LINE_POINTS - 1 ? 0f : Random.Range(-0.12f, 0.12f);
            lr.SetPosition(i, Vector2.Lerp(from, to, t) + side * jitter);
        }
    }

    public override void OnUnequip()
    {
        foreach (LineRenderer lr in lines)
            if (lr != null)
                Destroy(lr.gameObject);
        lines.Clear();
    }
}
