using UnityEngine;

// [암천창]
// TriggerItem. 쿨타임마다 랜덤 방향으로 MiningSpear를 소환.
// 창은 직선으로 관통하며 count에 따라 3/5/8회 관통 후 사라짐.
// 데미지는 마력의 80%/100%/120%.
public class SpearItem : TriggerItem
{
    public Spear spearPrefab;

    static readonly int[] pierceCounts = { 3, 5 };
    static readonly float[] damageRates = { 0.8f, 1.0f };

    int PierceCount => pierceCounts[Mathf.Clamp(count - 1, 0, pierceCounts.Length - 1)];
    float DamageRate => damageRates[Mathf.Clamp(count - 1, 0, damageRates.Length - 1)];

    public override void OnEquip(Player player)
    {
        UpdateItem();
        base.OnEquip(player);
    }

    public override void OnTrigger()
    {
        base.OnTrigger();

        Vector2 dir = Random.insideUnitCircle.normalized;
        float damage = Player.Instance.statMgr.MagicPower * DamageRate;

        var spear = Instantiate(spearPrefab, Player.Instance.transform.position, Quaternion.identity);
        spear.Init(dir, damage, PierceCount);
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        int pierce = pierceCounts[Mathf.Clamp(c - 1, 0, pierceCounts.Length - 1)];
        float rate = damageRates[Mathf.Clamp(c - 1, 0, damageRates.Length - 1)];
        return $"랜덤 방향으로 창 소환, {pierce}회 관통 후 소멸 (마력의 {rate * 100:0}%)";
    }
}
