using UnityEngine;

// 다중 발사 - 25% 확률로 멀티샷 발사
public class MultishotAbility : Ability, IPreAttack
{
    const float PROB = 0.25f;

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 멀티샷 발사";
    }

    public override void OnUnequip(Player player) { }

    public void OnPreAttack(Player player, Vector2 dir, int shotOrder)
    {
        // if (Random.value < PROB)
        //     player.weapon.RequestMulti(1);
    }
}
