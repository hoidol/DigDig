using UnityEngine;

// 일격탄 - 마지막 탄 발사 시 관통 +3
public class FinisherShotAbility : Ability, IBullet
{
    const int PIERCE_COUNT = 3;

    public override void OnEquip(Character player) { }
    public override void OnUnequip(Character player) { }

    public override string GetDescription(bool detail = false)
    {
        return $"마지막 탄 관통 +{PIERCE_COUNT}";
    }

    public void OnBulletFired(CharacterBulletObject bullet)
    {
        //if (Player.Instance.curBulletCount != 1) return; //마지막 탄있지 확인
        bullet.AddBehavior(new PierceBehavior(PIERCE_COUNT));
    }
}
