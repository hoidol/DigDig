using UnityEngine;

// 일격탄 - 마지막 탄 발사 시 크리티컬 확률 보너스
public class FinisherShotAbility : Ability, IBulletItem
{
    static readonly float[] critBonuses = { 50f, 60f, 70f };

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Player.Instance.curBulletCount != 1) return;
        bullet.AddBulletForce(new BonusCritChanceBehavior(critBonuses[count - 1]));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"마지막 탄 크리티컬 확률 +{critBonuses[c - 1]:0}%";
    }
}
