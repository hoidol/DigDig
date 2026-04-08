using System.Collections.Generic;
using UnityEngine;

public abstract class OrbitItemBase : Item
{
    public OrbitOrb orbPrefab;
    public float orbitRadius = 2f;

    protected List<OrbitOrb> orbs = new();

    public override void UpdateItem()
    {
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            OrbitOrb orb = Instantiate(orbPrefab, Player.Instance.transform);
            orb.transform.localPosition = Vector3.zero;
            orb.transform.localRotation = Quaternion.identity;
            orb.transform.Rotate(Vector3.forward * angleStep * i);
            orb.transform.Translate(orb.transform.up * orbitRadius, Space.World);
            orbs.Add(orb);
        }
    }

    public override void OnUnequip(Player player)
    {
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
    }
}
