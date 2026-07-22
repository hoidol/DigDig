using UnityEngine;

// 빠른 발사 - 25% 확률로 연속 발사
public class QuickFireAbility : Ability//, IAttack
{
    const float PROB = 0.25f;

    public override string GetDescription(bool detail = false)
    {
        return $"{PROB * 100:0}% 확률로 연속 발사";
    }

    public override void OnUnequip(Player player) { }

    public void OnAttack(Vector2 dir)
    {
        // if (Random.value < PROB)
        //     player.weapon.QueueExtraShot(1);
    }
}
