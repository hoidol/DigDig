using UnityEngine;

// [연쇄 곡괭이]
// 플레이어 총알로 광석을 파괴했을 때 40% 확률로 주변 광석 1개 연쇄 타격.
public class PickaxeItem : Item
{
    const float CHANCE = 60f;
    const int CHAIN_COUNT = 3;

    public float chainRadius = 3f;
    public LayerMask stoneLayer;

    public override void OnEquip()
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override void OnUnequip()
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override string GetDescription()
    {
        return $"총알로 광석 파괴 시 {CHANCE}% 확률로 주변 광석 {CHAIN_COUNT}개 연쇄 타격";
    }

    void OnDestroyedStone(DestroyedStoneEvent e)
    {
        if (Random.Range(0f, 100f) > CHANCE) return;

        Collider2D[] cols = Physics2D.OverlapCircleAll(
            e.stone.transform.position, chainRadius, stoneLayer);

        var candidates = new System.Collections.Generic.List<(Stone stone, float dist)>();
        foreach (var col in cols)
        {
            if (!col.TryGetComponent(out Stone stone)) continue;
            if (stone == e.stone) continue;
            float dist = Vector2.Distance(e.stone.transform.position, stone.transform.position);
            candidates.Add((stone, dist));
        }
        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));

        float damage = Character.Instance.statMgr.AttackPower/2f;
        int hitCount = Mathf.Min(CHAIN_COUNT, candidates.Count);
        for (int i = 0; i < hitCount; i++)
        {
            candidates[i].stone.TakeDamage(new DamageData { damage = damage });
            EffectManager.Instance.Play(EffectType.Spark, candidates[i].stone.transform.position);
        }
    }
}
