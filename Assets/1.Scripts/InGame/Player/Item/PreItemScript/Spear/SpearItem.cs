using UnityEngine;

// [암천창]
// TriggerItem. 쿨타임마다 랜덤 방향으로 Spear를 소환.
// 5회 관통, 데미지는 마력의 100%.
public class SpearItem : TriggerItem
{
    const int PIERCE_COUNT = 5;
    const float DAMAGE_RATE = 1.0f;

    public Spear spearPrefab;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
    }

    public override void OnTrigger()
    {
        base.OnTrigger();

        Vector2 dir = Random.insideUnitCircle.normalized;
        float damage = Player.Instance.statMgr.AttackPower * DAMAGE_RATE;

        var spear = Instantiate(spearPrefab, Player.Instance.transform.position, Quaternion.identity);
        spear.Init(dir, damage, PIERCE_COUNT);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"랜덤 방향으로 창 소환, {PIERCE_COUNT}회 관통 후 소멸 (마력의 {DAMAGE_RATE * 100:0}%)";
    }
}
