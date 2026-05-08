using System.Collections.Generic;
using UnityEngine;

// 아이템 transform 자체가 컨테이너 역할 (bodyCenterTr 자식으로 부착)
// OrbitOrb들은 이 transform의 자식으로 균등 배치되어 함께 회전
public abstract class OrbitItemBase : TriggerCycleItem
{
    public OrbitOrb orbPrefab;
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;

    protected List<OrbitOrb> orbs = new();

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        transform.SetParent(player.bodyCenterTr);
        transform.localRotation = Quaternion.identity;
        transform.position = player.bodyCenterTr.position;
        //transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        RebuildOrbs();
        UpdateItem();
    }

    public override void UpdateItem()
    {
        RebuildOrbs();
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

            orb.transform.localPosition = localPos;
            orb.transform.up = transform.position - orb.transform.position;
            orbs.Add(orb);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
    }

    public override void OnUnequip(Player player)
    {
        base.OnUnequip(player);
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
    }
}
