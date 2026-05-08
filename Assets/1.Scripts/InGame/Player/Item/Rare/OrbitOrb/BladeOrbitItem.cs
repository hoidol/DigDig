using UnityEngine;

// [칼날 궤도]
// OrbitItemBase. 칼날 OrbitOrb(count개)가 플레이어 주변을 회전하며 닿는 적/광석에 피해.
// 데미지는 마력의 50%/70%/100%. UpdateItem 시 모든 orb.damage를 즉시 갱신.
public class BladeOrbitItem : OrbitItemBase
{
    static readonly float[] damageRates = { 0.5f, 0.7f, 1.0f };

    float DamageRate => damageRates[Mathf.Clamp(count - 1, 0, damageRates.Length - 1)];

    public override void OnActivate() { }
    public override void OnDeactivate() { }

    public override void UpdateItem()
    {
        base.UpdateItem(); // RebuildOrbs 호출 → orbs 채워짐
        ApplyDamage();
    }

    void ApplyDamage()
    {
        float damage = Player.Instance.statMgr.MagicPower * DamageRate;
        foreach (var orb in orbs)
            orb.damage = damage;
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        float rate = damageRates[Mathf.Clamp(c - 1, 0, damageRates.Length - 1)];
        return $"칼날 {c}개가 주변을 회전하며 적에게 피해 (마력의 {rate * 100:0}%)";
    }
}
