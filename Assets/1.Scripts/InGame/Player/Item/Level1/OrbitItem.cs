using System.Collections.Generic;
using UnityEngine;

public class OrbitItem : Item 
{
    public float[] damages = {3,4,5};
    public OrbitOrb orbPrefab;
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;

    protected List<OrbitOrb> orbs = new();
    // protected virtual int OrbCount => count;
    
    public override void OnEquip()
    {
        base.OnEquip();
        transform.SetParent(Player.Instance.bodyCenterTr);
        transform.localRotation = Quaternion.identity;
        transform.position = Player.Instance.bodyCenterTr.position;
        //transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        RebuildOrbs();
        UpdateItem();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
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
            orb.damage= damages[count-1];
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