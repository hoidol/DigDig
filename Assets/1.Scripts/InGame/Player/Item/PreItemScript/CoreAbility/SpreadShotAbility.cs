using UnityEngine;

// 산탄 - 20% 확률로 확산탄 발사
public class SpreadShotAbility : Ability, IPreAttack
{
    const float PROB = 0.20f;

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 확산탄 발사";
    }

    public override void OnUnequip(Player player) { }

    public void OnPreAttack(Player player, Vector2 dir)
    {
        // if (Random.value < PROB)
        // player.weapon.RequestSpread(1);
    }
}
