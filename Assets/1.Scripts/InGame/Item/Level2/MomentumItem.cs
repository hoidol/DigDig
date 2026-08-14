using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

//궤도를 2개 되고 빨라지고 커짐
public class MomentumItem : TriggerCycleItem
{
    public float damage =  4;
    float baseCoolTime =5;
    float baseActiveTime = 8;
    public OrbitOrb orbPrefab;
    public float orbitRadius = 3.5f; //괘적이 넓음 
    float orbitSpeed = 130f;

    public float consumeTime = 5;
    protected List<OrbitOrb> orbs = new();
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
        coolTime = baseCoolTime;
        activeTime = baseActiveTime;
    }

    protected void RebuildOrbs()
    {
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
        int orbCount = count;
        float angleStep = 360f / orbCount;
        for (int i = 0; i < orbCount; i++)
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
        return $"거대 궤도를 소환합니다.\n{consumeTime}초마다 체력 {itemData.consumeHp} 감소";
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