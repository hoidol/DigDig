using UnityEngine;

// 광석 관통 - 25% 확률로 관통탄 발사 (관통 +1)
public class MiningPierceAbility : Ability, IBullet
{
    const float PROB = 0.25f;

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 관통탄 발사 (관통 +1)";
    }

    public override void OnUnequip(Character player) { }

    public void OnBulletFired(CharacterBulletObject bullet)
    {
        if (Random.value < PROB)
            bullet.AddBehavior(new PierceBehavior(1));
    }
}
