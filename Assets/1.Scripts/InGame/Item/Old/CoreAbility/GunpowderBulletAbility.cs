using UnityEngine;

// 화약탄 - 15% 확률로 폭발탄 발사
public class GunpowderBulletAbility : Ability, IBullet
{
    const float PROB = 0.15f;
    const float RADIUS = 1.0f;
    const float DAMAGE_RATIO = 0.8f;

    public override void OnEquip(KingSlime player) { }
    public override void OnUnequip(KingSlime player) { }

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 폭발탄 발사";
    }

    public void OnBulletFired(KingSlimeBulletObject bullet)
    {
        if (Random.value > PROB) return;
        float dmg = KingSlime.Instance.statMgr.AttackPower * DAMAGE_RATIO;
        bullet.AddBehavior(new BoomBehaviour(RADIUS, dmg, 5, LayerMask.GetMask("Hittable")));
    }
}
