using UnityEngine;

// 고무탄 - 확률로 발사된 총알에 튕김 부여
public class BoundShootAbility : Ability, IBulletItem
{
    static readonly float[] probs = { 0.10f, 0.15f, 0.20f, 0.25f, 0.35f };

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1) c = count;
        if (c <= 0) c = 1;
        return $"{probs[c - 1] * 100:0}% 확률로 도탄 발사 (튕김 +1)";
    }

    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Random.value < probs[count - 1])
            bullet.AddBehavior(new BounceBehavior(1));
    }
}
