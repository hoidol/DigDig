using UnityEngine;

public class PoisonBulletItem : Item, IBulletItem
{
    static readonly float[] durations = { 3f, 4f, 5f };
    static readonly float[] dpsValues = { 2f, 3f, 4f };
    static readonly float[] chances = { 5f, 8f, 12f };

    float dpsValue;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        if (c < 1) c = 1;
        return $"{chances[c - 1]}% 확률로 맹독탄 발사";
    }

    public override void OnEquip(Player player) { UpdateEnhancement(); }
    public override void OnUnequip(Player player) { }

    public override void UpdateEnhancement()
    {
        dpsValue = Player.Instance.statMgr.AttackPower * 0.01f * dpsValues[count - 1];
        if (dpsValue < 1f) dpsValue = 1f;
    }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Random.Range(0f, 100f) < chances[count - 1])
            bullet.AddBehavior(new PoisonOnHitBehavior(durations[count - 1], dpsValue));
    }
}
