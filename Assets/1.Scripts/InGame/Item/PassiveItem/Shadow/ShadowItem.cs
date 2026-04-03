using System.Collections.Generic;
using UnityEngine;

public class ShadowItem : Item, IAttackItem
{
    public Shadow shadowPrefab;
    public float damageRate = 0.3f;

    List<Shadow> shadows = new();

    public override void OnEquip(Player player) => UpdateItem();

    public override void UpdateItem()
    {
        foreach (var s in shadows) Destroy(s.gameObject);
        shadows.Clear();

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            Shadow shadow = Instantiate(shadowPrefab);
            // count가 늘면 주위를 원형으로 배치
            float angle = angleStep * i * Mathf.Deg2Rad;
            shadow.offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2f;
            shadows.Add(shadow);
        }
    }

    public override void OnUnequip(Player player)
    {
        foreach (var s in shadows) Destroy(s.gameObject);
        shadows.Clear();
    }

    public void OnAttack(Player player, Vector2 dir)
    {
        float damage = player.playerStatMgr.AttackPower * damageRate;
        foreach (var s in shadows)
            s.Shoot(dir, damage);
    }
}
