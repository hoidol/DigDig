using UnityEngine;

// 고무탄 - 25% 확률로 도탄 발사 (튕김 +1)
public class BoundShootAbility : Ability, IBulletItem
{
    const float PROB = 0.25f;

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 도탄 발사 (튕김 +1)";
    }

    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Random.value < PROB)
            bullet.AddBehavior(new BounceBehavior(1));
    }
}
