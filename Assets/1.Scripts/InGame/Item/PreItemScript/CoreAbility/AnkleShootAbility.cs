using UnityEngine;

// 앵클샷 - 적중 시 40% 확률로 슬로우
public class AnkleShootAbility : Ability, IBullet
{
    const float CHANCE = 0.40f;
    const float DURATION = 3f;

    public override void OnEquip(Character player) { }
    public override void OnUnequip(Character player) { }

    public override string GetDescription(bool detail = false)
    {
        return $"적중 시 {CHANCE * 100:0}% 확률로 {DURATION}초 슬로우";
    }

    public void OnBulletFired(CharacterBulletObject bullet)
    {
        bullet.AddBehavior(new SlowOnHitBehavior(CHANCE, DURATION));
    }
}
