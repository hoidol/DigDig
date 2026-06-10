using UnityEngine;

// 맹독탄: 30% 확률로 독 상태이상 부여
public class PoisonBulletItem : Item, IBullet
{
    const float CHANCE = 30f;
    const float DURATION = 3f;

    float dpsValue;

    public override string GetDescription(bool detail = false)
    {
        return $"{CHANCE}% 확률로 맹독탄 발사";
    }

    public override void OnEquip(Player player) { UpdateEnhancement(); }
    public override void OnUnequip(Player player) { }

    public override void UpdateEnhancement()
    {
        dpsValue = Player.Instance.statMgr.AttackPower * 0.02f;
        if (dpsValue < 1f) dpsValue = 1f;
    }

    public void OnBulletFired(PlayerBulletObject bullet)
    {
        if (Random.Range(0f, 100f) < CHANCE)
            bullet.AddBehavior(new PoisonOnHitBehavior(DURATION, dpsValue));
    }
}
