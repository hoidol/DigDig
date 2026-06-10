// 회복 증폭 - 힐로 회복되는 양 30% 추가 증가
public class HealBoostAbility : Ability
{
    const float MULTIPLIER = 1.3f;

    public override string GetDescription(bool detail = false)
    {
        return $"체력 회복 시 {(MULTIPLIER - 1) * 100:0}% 추가 회복";
    }

    public override void OnEquip(Player player)
    {
        player.healMultiplier *= MULTIPLIER;
    }

    public override void OnUnequip(Player player)
    {
        player.healMultiplier /= MULTIPLIER;
    }
}
