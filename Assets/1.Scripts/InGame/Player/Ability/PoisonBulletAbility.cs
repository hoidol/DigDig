// 독탄 - 모든 총알 적중 시 독 DoT 부여 (0.5초마다 데미지)
using UnityEngine;

public class PoisonBulletAbility : Ability, IBulletItem
{
    static readonly float[] durations = { 3f, 4f, 5f };
    static readonly float[] dpsValues = { 2f, 3f, 4f };

    float dpsValue;
    public float[] chances = { 10, 20, 30 };
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"{chances[c - 1]}% 확률로 독탄 발사";
    }
    public override void OnEquip(Player player) { UpdateEnhancement(); }
    public override void OnUnequip(Player player) { }
    public override void UpdateEnhancement()
    {
        dpsValue = Player.Instance.statMgr.AttackPower * 0.01f * dpsValues[count - 1];
        if (dpsValue <= 1)
            dpsValue = 1;
    }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Random.Range(0f, 100) < chances[count - 1])
            bullet.AddBehavior(new PoisonOnHitBehavior(durations[count - 1], dpsValue));
    }
}
