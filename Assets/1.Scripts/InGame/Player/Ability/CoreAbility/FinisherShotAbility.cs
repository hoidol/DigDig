using UnityEngine;

// 일격탄 - 마지막 탄 발사 시 관통 부여
public class FinisherShotAbility : Ability, IBulletItem
{
    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Player.Instance.curBulletCount != 1) return;
        bullet.AddBehavior(new PierceBehavior(count));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"마지막 탄 관통 +{c}";
    }
}
