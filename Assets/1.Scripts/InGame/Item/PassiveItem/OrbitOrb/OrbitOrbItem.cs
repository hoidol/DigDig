using System.Collections.Generic;
using UnityEngine;

public class OrbitOrbEffect : Item, IPassiveItem
{
    public OrbitOrb orbPrefab;
    List<OrbitOrb> orbitOrbs = new List<OrbitOrb>();

    public override void OnEquip(Player player) { }

    public override void UpdateItem()
    {
        for (int i = 0; i < orbitOrbs.Count; i++)
        {
            Destroy(orbitOrbs[i]);
        }
        orbitOrbs.Clear();
        float angle = 360f / (float)count;
        for (int i = 0; i < count; i++)
        {
            OrbitOrb orb = Instantiate(orbPrefab, Player.Instance.transform);
            orbitOrbs.Add(orb);

            orb.transform.localPosition = Vector3.zero;
            orb.transform.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * angle * i;
            orb.transform.Rotate(rotVec);
            orb.transform.Translate(orb.transform.up * 10f, Space.World);
        }
    }

    public override void OnUnequip(Player player)
    {
        for (int i = 0; i < orbitOrbs.Count; i++)
        {
            Destroy(orbitOrbs[i]);
        }
    }
}
