using UnityEngine;

// [연쇄 곡괭이]
// 플레이어 총알로 광석을 파괴했을 때 일정 확률(30%/50%/100%)로 주변 광석을 count개 연쇄 타격.
// OreStoneDestroyedEvent를 구독하며, cause가 PlayerBullet인 경우에만 발동.
// 주변 광석을 거리순으로 정렬 후 가까운 순서대로 타격.
public class ChainPickaxeItem : Item
{
    public float chainRadius = 5f;
    public LayerMask oreLayer;

    static readonly float[] chances = { 30f, 50f, 100f };

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        float chance = chances[Mathf.Clamp(c - 1, 0, chances.Length - 1)];
        return $"총알로 광석 파괴 시 {chance}% 확률로 주변 광석 {c}개 연쇄 타격";
    }

    void OnOreDestroyed(OreStoneDestroyedEvent e)
    {
        if (e.lastDamage == null || e.lastDamage.cause == null) return;
        if (e.lastDamage.cause.GetComponent<PlayerBullet>() == null) return;
        if (Random.Range(0f, 100f) > chances[Mathf.Clamp(count - 1, 0, chances.Length - 1)]) return;

        Collider2D[] cols = Physics2D.OverlapCircleAll(
            e.oreStone.transform.position, chainRadius, oreLayer);

        // 거리순 정렬 후 count개 타격
        var candidates = new System.Collections.Generic.List<(OreStone ore, float dist)>();
        foreach (var col in cols)
        {
            if (!col.TryGetComponent(out OreStone ore)) continue;
            if (ore == e.oreStone) continue;
            float dist = Vector2.Distance(e.oreStone.transform.position, ore.transform.position);
            candidates.Add((ore, dist));
        }
        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));

        float damage = Player.Instance.statMgr.MagicPower;
        int hitCount = Mathf.Min(count, candidates.Count);
        for (int i = 0; i < hitCount; i++)
        {
            candidates[i].ore.TakeDamage(new DamageData { damage = damage });
            EffectManager.Instance.Play(EffectType.Spark, candidates[i].ore.transform.position);
        }

    }
}
