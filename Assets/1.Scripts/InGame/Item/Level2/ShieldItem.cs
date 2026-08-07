using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//2개 되고 빨라지고 커짐
public class ShieldItem : TriggerCycleItem
{
    float[] damages = { 2, 2, 2 };
    float[] cooltimes = { 5, 5, 5 };
    float[] activeTimes = { 8, 8, 8 };
    public ShieldOrbitOrb shieldOrbitOrbPrefab;
    public float orbitRadius = 2.5f; //괘적이 넓음 
    public float orbitSpeed = 70f;

    protected List<ShieldOrbitOrb> orbs = new();
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
        coolTime = cooltimes[count - 1];
        activeTime = activeTimes[count - 1];
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

            ShieldOrbitOrb orb = Instantiate(shieldOrbitOrbPrefab, transform);
            orb.damage = damages[count - 1];
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