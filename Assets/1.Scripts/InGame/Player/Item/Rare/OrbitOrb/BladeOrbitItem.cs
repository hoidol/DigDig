using UnityEngine;

// [칼날 궤도]
// OrbitItemBase. 칼날 2개가 플레이어 주변을 회전하며 닿는 적/광석에 피해.
// 데미지는 마력의 50%.
public class BladeOrbitItem : OrbitItemBase
{
    const float DAMAGE_RATE = 0.5f;

    protected override int OrbCount => 2;

    public override void OnActivate() { }
    public override void OnDeactivate() { }

    public override void UpdateItem()
    {
        base.UpdateItem();
        ApplyDamage();
    }

    void ApplyDamage()
    {
        float damage = Player.Instance.statMgr.MagicPower * DAMAGE_RATE;
        foreach (var orb in orbs)
            orb.damage = damage;
    }

    public override string GetDescription(bool detail = false)
    {
        return $"칼날 2개가 주변을 회전하며 적에게 피해 (마력의 {DAMAGE_RATE * 100:0}%)";
    }
}
