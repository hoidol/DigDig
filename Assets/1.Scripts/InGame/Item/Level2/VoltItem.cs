using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//2개 되고 빨라지고 커짐
public class VoltItem : TriggerCycleItem
{
    float damage = 2;

    float baseCooltime = 5;
    float baseActiveTime = 8;

    float voltDamage = 3;
    float voltChance = 0.3f;
    float voltRadiuse = 1.5f;

    public VoltOrbitOrb voltOrbitOrbPrefab;
    float orbitRadius = 2.2f;
    float orbitSpeed = 60f;

    protected List<VoltOrbitOrb> orbs = new();
    // protected virtual int OrbCount => count;

    public override void OnEquip()
    {
        transform.SetParent(Character.Instance.transform);
        transform.localRotation = Quaternion.identity;
        transform.position = Character.Instance.transform.position;

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
        coolTime = baseCooltime;
        activeTime = baseActiveTime;
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
            orb.damage = damage;
            orb.voltDamage = voltDamage;
            orb.voltChance = voltChance;
            orb.voltRadius = voltRadiuse;


            orb.transform.localPosition = localPos;
            orb.transform.up = transform.position - orb.transform.position;
            orbs.Add(orb);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
    }


    public override void OnActivate()
    {
        Character.Instance.AddHp(-itemData.consumeHp);
        RebuildOrbs();
    }

    public override void OnDeactivate()
    {
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
    }

}