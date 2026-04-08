using UnityEngine;

public class ChainPickaxeItem : Item
{
    public float chainRadius = 5f;
    public LayerMask oreLayer;

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    void OnOreDestroyed(OreStoneDestroyedEvent e)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            e.oreStone.transform.position, chainRadius, oreLayer);

        OreStone nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            if (!col.TryGetComponent(out OreStone ore)) continue;
            if (ore == e.oreStone) continue;

            float dist = Vector2.Distance(e.oreStone.transform.position, ore.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = ore;
            }
        }

        if (nearest != null)
            nearest.TakeDamage(new DamageData { damage = Player.Instance.statMgr.AttackPower * count });
    }
}
