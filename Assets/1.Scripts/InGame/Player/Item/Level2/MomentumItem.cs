using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//2개 되고 빨라지고 커짐
public class MomentumItem : Item 
{
    public float[] damages = {5,5,5};
    public OrbitOrb orbPrefab;
    public float orbitRadius = 3.5f; //괘적이 넓음 
    public float orbitSpeed = 200f;

    public float consumeTime = 5;
    protected System.Collections.Generic.List<OrbitOrb> orbs = new();
    // protected virtual int OrbCount => count;
    
    public override void OnEquip()
    {
        transform.SetParent(Player.Instance.bodyCenterTr);
        transform.localRotation = Quaternion.identity;
        transform.position = Player.Instance.bodyCenterTr.position;
        
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
        int orbCount = count*2;
        float angleStep = 360f / orbCount;
        for (int i = 0; i < orbCount; i++)
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

    float timer;
    void Update()
    {
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
         timer += Time.deltaTime;
        if(timer >= consumeTime)
        {
            Player.Instance.AddHp(-itemData.consumeHp);
        }
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"거대 궤도를 소환합니다.\n{consumeTime}초마다 체력 {itemData.consumeHp} 감소";
    }
    
}