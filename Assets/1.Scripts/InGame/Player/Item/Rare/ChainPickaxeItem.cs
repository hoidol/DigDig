using UnityEngine;

// [연쇄 곡괭이]
// 플레이어 총알로 광석을 파괴했을 때 40% 확률로 주변 광석 1개 연쇄 타격.
public class ChainPickaxeItem : Item
{
    const float CHANCE = 40f;
    const int CHAIN_COUNT = 1;

    public float chainRadius = 5f;
    public LayerMask oreLayer;

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override string GetDescription(bool detail = false)
    {
        return $"총알로 광석 파괴 시 {CHANCE}% 확률로 주변 광석 {CHAIN_COUNT}개 연쇄 타격";
    }

    void OnDestroyedStone(DestroyedStoneEvent e)
    {
        if (e.lastDamage == null || e.lastDamage.cause == null) return;
        if (e.lastDamage.cause.GetComponent<PlayerBullet>() == null) return;
        if (Random.Range(0f, 100f) > CHANCE) return;

        Collider2D[] cols = Physics2D.OverlapCircleAll(
            e.oreStone.transform.position, chainRadius, oreLayer);

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
        int hitCount = Mathf.Min(CHAIN_COUNT, candidates.Count);
        for (int i = 0; i < hitCount; i++)
        {
            candidates[i].ore.TakeDamage(new DamageData { damage = damage });
            EffectManager.Instance.Play(EffectType.Spark, candidates[i].ore.transform.position);
        }
    }
}
