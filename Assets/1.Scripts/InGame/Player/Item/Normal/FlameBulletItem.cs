using UnityEngine;

public class FlameBulletItem : Item, IBulletItem
{
    static readonly float duration = 3f;
    static readonly float dpsValue = 2f;
    static readonly float chance = 20f;


    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        if (c < 1) c = 1;
        return $"{chance}% 확률로 불꽃탄 발사";
    }

    public override void OnEquip(Player player) { UpdateEnhancement(); }
    public override void OnUnequip(Player player) { }


    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Random.Range(0f, 100f) < chance)
            bullet.AddBehavior(new BurnOnHitBehavior(duration, dpsValue));
    }
}
