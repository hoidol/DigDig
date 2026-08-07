using UnityEngine;

// 화약탄 - 15% 확률로 폭발탄 발사
public class GunpowderBulletAbility : Ability, IBullet
{
    const float PROB = 0.15f;
    const float RADIUS = 1.0f;
    const float DAMAGE_RATIO = 0.8f;

    public override void OnEquip(Character player) { }
    public override void OnUnequip(Character player) { }

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 폭발탄 발사";
    }

    public void OnBulletFired(CharacterBulletObject bullet)
    {
        if (Random.value > PROB) return;
        float dmg = Character.Instance.statMgr.AttackPower * DAMAGE_RATIO;
        bullet.AddBehavior(new BoomBehaviour(RADIUS, dmg, LayerMask.GetMask("Hittable")));
    }
}
