using System.Collections.Generic;
using UnityEngine;

public class FenceEnemyPattern : EnemyPattern
{
    [SerializeField] OreStoneUnbreakable oreStonePrefab;
    [SerializeField] float radius = 15f;
    [SerializeField] float spacing = 1f;

    readonly List<OreStoneUnbreakable> oreStoneUnbreakables = new();

    public override void StartPattern(EnemyPatternData enemyPatternData)
    {


        Vector2 center = Player.Instance.transform.position;
        int count = Mathf.CeilToInt(2f * Mathf.PI * radius / spacing);

        for (int i = 0; i < count; i++)
        {
            float angle = 2f * Mathf.PI * i / count;
            Vector3 pos = new(
                center.x + radius * Mathf.Cos(angle),
                center.y + radius * Mathf.Sin(angle),
                0f
            );
            oreStoneUnbreakables.Add(OreStoneUnbreakable.Get(oreStonePrefab, pos, null));
        }
    }

    public override void EndPattern()
    {
        foreach (var ore in oreStoneUnbreakables)
            if (ore != null) ore.Return();
        oreStoneUnbreakables.Clear();
        base.EndPattern();
    }
}