using System.Collections.Generic;
using UnityEngine;

public class OrbitItem : TriggerCycleItem
{
    float damage = 2;
    float baseCoolTime = 5;
    float baseActiveTime = 8;

    public OrbitOrb orbPrefab;
    public float orbitRadius = 2f;
    public float orbitSpeed = 110f;

    protected List<OrbitOrb> orbs = new();

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
        coolTime = baseCoolTime;
        activeTime = baseActiveTime;
    }

    protected void RebuildOrbs()
    {
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float rad = angleStep * i * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;

            OrbitOrb orb = Instantiate(orbPrefab, transform);
            orb.damage = damage;
            orb.transform.localPosition = localPos;
            orb.transform.up = transform.position - orb.transform.position;
            orbs.Add(orb);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
    }


    public override string GetDescription()
    {
        return $"궤도탄 1개 추가";
        //return TranslateManager.GetText($"{key}_Desc");
    }


}