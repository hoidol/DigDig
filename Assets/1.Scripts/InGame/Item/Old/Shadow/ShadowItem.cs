using System.Collections.Generic;
using UnityEngine;

public class ShadowItem : Item//, IAttack
{
    public Shadow shadowPrefab;
    public float damageRate = 0.3f;

    List<Shadow> shadows = new();


    public override void UpdateItem()
    {
        foreach (var s in shadows) Destroy(s.gameObject);
        shadows.Clear();

        Shadow shadow = Instantiate(shadowPrefab);
        shadow.offset = Vector2.right * 2f;
        shadows.Add(shadow);
    }

    public override void OnUnequip()
    {
        foreach (var s in shadows) Destroy(s.gameObject);
        shadows.Clear();
    }

    public void OnAttack(Vector2 dir)
    {
        float damage = Character.Instance.statMgr.AttackPower * damageRate;
        foreach (var s in shadows)
            s.Shoot(dir, damage);
    }
}
