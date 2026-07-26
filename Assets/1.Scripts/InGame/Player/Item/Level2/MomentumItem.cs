using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//2개 되고 빨라지고 커짐
public class MomentumItem : TriggerCycleItem
{
    public float[] damages = { 4, 4, 4 };
    float[] cooltimes = { 5, 5, 5 };
    float[] activeTimes = { 8, 8, 8 };
    public OrbitOrb orbPrefab;
    public float orbitRadius = 3.5f; //괘적이 넓음 
    float orbitSpeed = 130f;

    public float consumeTime = 5;
    protected System.Collections.Generic.List<OrbitOrb> orbs = new();
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
        coolTime = cooltimes[count - 1];
        activeTime = activeTimes[count - 1];
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

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"거대 궤도를 소환합니다.\n{consumeTime}초마다 체력 {itemData.consumeHp} 감소";
    }

    public override void OnActivate()
    {
        Player.Instance.AddHp(-itemData.consumeHp);
        RebuildOrbs();
    }

    public override void OnDeactivate()
    {
        foreach (var orb in orbs) Destroy(orb.gameObject);
        orbs.Clear();
    }
}