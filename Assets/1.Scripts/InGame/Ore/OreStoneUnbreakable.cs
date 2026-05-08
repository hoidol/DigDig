using System.Collections.Generic;
using UnityEngine;

public class OreStoneUnbreakable : MonoBehaviour, IHittable
{
    public Transform Transform => transform;

    public bool CanHit()
    {
        return true;
    }

    public void TakeDamage(DamageData damageData)
    {
    }

    static readonly Stack<OreStoneUnbreakable> pool = new();

    public static OreStoneUnbreakable Get(OreStoneUnbreakable prefab, Vector3 pos, Transform parent)
    {
        OreStoneUnbreakable ore = pool.Count > 0 ? pool.Pop() : Instantiate(prefab, parent);
        ore.transform.SetParent(parent);
        ore.transform.position = pos;
        ore.gameObject.SetActive(true);
        return ore;
    }
    public void Destroyed()
    {
        Return();
    }

    public void Return()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        pool.Push(this);
    }
}
