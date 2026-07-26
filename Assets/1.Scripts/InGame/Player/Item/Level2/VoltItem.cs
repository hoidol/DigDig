using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//2개 되고 빨라지고 커짐
public class VoltItem : Item
{
    public float[] damages = { 5, 5, 5 };

    public float[] voltDamages = { 4f, 5f, 6f };
    public float[] voltChances = { 0.3f, 0.5f, 0.7f };
    public float[] voltRadiuses = { 2f, 2f, 2f };

    public VoltOrbitOrb voltOrbitOrbPrefab;
    public float orbitRadius = 2.2f;
    public float orbitSpeed = 170f;

    protected List<VoltOrbitOrb> orbs = new();
    // protected virtual int OrbCount => count;

    public override void OnEquip()
    {
        transform.SetParent(Player.Instance.transform);
        transform.localRotation = Quaternion.identity;
        transform.position = Player.Instance.transform.position;

        base.OnEquip();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
    }

    public override void UpdateItem()
    {
        base.UpdateItem();
        RebuildOrbs();
    }

    protected void RebuildOrbs()
    {
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
        int orbCount = count + 1;
        float angleStep = 360f / orbCount;
        for (int i = 0; i < orbCount; i++)
        {
            float rad = angleStep * i * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;

            VoltOrbitOrb orb = Instantiate(voltOrbitOrbPrefab, transform);
            orb.damage = damages[count - 1];
            orb.voltDamage = voltDamages[count - 1];
            orb.voltChance = voltChances[count - 1];
            orb.voltRadius = voltRadiuses[count - 1];


            orb.transform.localPosition = localPos;
            orb.transform.up = transform.position - orb.transform.position;
            orbs.Add(orb);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
    }


}