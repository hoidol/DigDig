using System.Collections.Generic;
using UnityEngine;

public class UnbreakableStone : OreStone, IHittable, ITile
{
    public override void Init(int level, Color color, Vector2Int[,] idxArr)//, Vector2Int gridPos
    {
        RegisterTile(idxArr);
    }

    public override bool CanHit()
    {
        return false;
    }

    public override void TakeDamage(DamageData damageData)
    {

    }

    static UnbreakableStone unbreakableStonePrefab;
    static readonly Stack<UnbreakableStone> pool = new();

    public static UnbreakableStone Get(Vector3 pos, Transform parent)
    {
        if (unbreakableStonePrefab == null)
            unbreakableStonePrefab = Resources.Load<UnbreakableStone>("Prefabs/UnbreakableStone");

        UnbreakableStone ore = pool.Count > 0 ? pool.Pop() : Instantiate(unbreakableStonePrefab, parent);
        ore.transform.position = pos;
        ore.gameObject.SetActive(true);
        return ore;
    }

    public override void ReleaseTile()
    {
        MapManager.ReleaseTile(tileIndexArr);

        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        pool.Push(this);
    }

    public override void ApplyStatusEffect(StatusEffect effect)
    {

    }

}
