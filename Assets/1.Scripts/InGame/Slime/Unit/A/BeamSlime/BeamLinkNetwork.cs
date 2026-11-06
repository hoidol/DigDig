using System.Collections.Generic;
using UnityEngine;

// 광선 슬라임(BeamSlime) 레이저 연결 관리. 필드의 광선 슬라임 전체를 하나의 망으로 보고 프레임마다 다시 계산
// - 연결: 타일 위 광선 슬라임 모든 쌍을 연결 (n마리 → n(n-1)/2개). 레이저 두께 = 두 슬라임 lineWidth(머지 레벨별) 평균
// - 피해: TICK_INTERVAL(0.25초 고정)마다 레이저마다 닿은 적 전부에게 피해 = 두 슬라임 공격력 평균 × 배율
//         배율 = 1 - 감소율 × (전체 레이저 수 - 1), 최소 minLineDamageMul (감소율은 두 슬라임 중 낮은 값)
//   → 같은 적이 레이저 2개에 걸쳐 있으면 2번 맞음. 누적 데미지는 두 슬라임에 반씩
// - 타일 아이템 발사 효과(OnFired)는 탄이 아니라서 적용 안 됨
public static class BeamLinkNetwork
{
    struct Link
    {
        public BeamSlime a;
        public BeamSlime b;
    }

    public const float TICK_INTERVAL = 0.5f;

    static readonly List<BeamSlime> beams = new List<BeamSlime>();
    static readonly List<Link> links = new List<Link>();
    static readonly List<LineRenderer> linePool = new List<LineRenderer>();
    static readonly AllyUnitDamageData damageData = new AllyUnitDamageData();
    static int lastTickFrame = -1;
    static float tickTimer;

    public static void Register(BeamSlime beam)
    {
        if (!beams.Contains(beam))
            beams.Add(beam);
    }

    public static void Unregister(BeamSlime beam)
    {
        beams.Remove(beam);
        // 남은 광선 슬라임이 없으면 Tick이 안 불리므로 레이저를 여기서 숨김 (남아 있으면 다음 Tick에 다시 켜짐)
        // 씬 종료 중 싱글톤 접근을 피하려고 다시 계산하지 않고 끄기만 함
        foreach (LineRenderer line in linePool)
        {
            if (line != null)
                line.gameObject.SetActive(false);
        }
    }

    public static void Tick()
    {
        if (lastTickFrame == Time.frameCount)
            return;
        lastTickFrame = Time.frameCount;

        beams.RemoveAll(b => b == null);
        RebuildLinks();
        UpdateLines();
        UpdateDamage();
    }

    static void RebuildLinks()
    {
        links.Clear();
        List<BeamSlime> candidates = new List<BeamSlime>();
        foreach (BeamSlime beam in beams)
        {
            beam.linkCount = 0;
            if (beam.CanLink)
                candidates.Add(beam);
        }
        if (candidates.Count < 2)
            return;

        // 모든 쌍 연결
        for (int i = 0; i < candidates.Count; i++)
        {
            for (int j = i + 1; j < candidates.Count; j++)
            {
                links.Add(new Link { a = candidates[i], b = candidates[j] });
                candidates[i].linkCount++;
                candidates[j].linkCount++;
            }
        }
    }

    static void UpdateLines()
    {
        linePool.RemoveAll(l => l == null); // 씬 전환으로 파괴된 것

        for (int i = 0; i < links.Count; i++)
        {
            if (i >= linePool.Count)
                linePool.Add(CreateLine(links[i].a));

            LineRenderer line = linePool[i];
            line.gameObject.SetActive(true);
            line.startWidth = line.endWidth = GetWidth(links[i]);
            line.startColor = line.endColor = links[i].a.lineColor;
            line.SetPosition(0, links[i].a.transform.position);
            line.SetPosition(1, links[i].b.transform.position);
        }
        for (int i = links.Count; i < linePool.Count; i++)
            linePool[i].gameObject.SetActive(false);
    }

    static float GetWidth(Link link) => (link.a.LineWidth + link.b.LineWidth) * 0.5f;

    static LineRenderer CreateLine(BeamSlime owner)
    {
        LineRenderer line = new GameObject("BeamLine").AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.material = owner.lineMaterial != null ? owner.lineMaterial : new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 10;
        return line;
    }

    static void UpdateDamage()
    {
        if (links.Count == 0)
        {
            tickTimer = 0; // 새로 연결되면 TICK_INTERVAL 뒤 첫 피해
            return;
        }
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
            return;

        tickTimer += Time.deltaTime;
        if (tickTimer < TICK_INTERVAL)
            return;
        tickTimer -= TICK_INTERVAL;

        foreach (Link link in links)
            HitLine(link);
    }

    static void HitLine(Link link)
    {
        Vector2 from = link.a.transform.position;
        Vector2 to = link.b.transform.position;
        Vector2 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0f)
            return;

        float reduction = Mathf.Min(link.a.damageReductionPerLine, link.b.damageReductionPerLine);
        float mul = Mathf.Max(Mathf.Max(link.a.MinLineDamageMul, link.b.MinLineDamageMul), 1f - reduction * (links.Count - 1));
        float damage = (link.a.AttackPower + link.b.AttackPower) * 0.5f * mul;
        bool slow = link.a.slowOnLine || link.b.slowOnLine;
        float slowDuration = Mathf.Max(link.a.slowDuration, link.b.slowDuration);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(from, GetWidth(link) * 0.5f, dir / dist, dist, link.a.targetLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            IHittable hittable = hit.collider.GetComponent<IHittable>();
            if (hittable == null)
                continue;

            damageData.Init(link.a);
            damageData.damage = damage;
            damageData.attackType = link.a.AttackType;
            link.a.AccumulateDamage(damage * 0.5f);
            link.b.AccumulateDamage(damage * 0.5f);
            hittable.TakeDamage(damageData);

            if (slow && hittable is Enemy enemy && enemy.gameObject.activeInHierarchy)
                enemy.GetComponent<StatusEffectHandler>()?.Apply(new SlowEffect(slowDuration));
        }
    }
}
