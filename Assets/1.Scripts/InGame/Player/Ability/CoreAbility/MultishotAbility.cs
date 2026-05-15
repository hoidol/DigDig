using UnityEngine;

// 다중 발사 - 확률로 멀티샷 발사
public class MultishotAbility : Ability, IPreAttack
{
    static readonly float[] probs = { 0.10f, 0.15f, 0.20f, 0.25f, 0.35f };

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1) c = count;
        if (c <= 0) c = 1;
        return $"{probs[c - 1] * 100}% 확률로 멀티샷 발사";
    }

    public override void OnUnequip(Player player) { }

    public void OnPreAttack(Player player, Vector2 dir)
    {
        if (Random.value < probs[count - 1])
            player.weapon.RequestMulti(1);
    }
}
