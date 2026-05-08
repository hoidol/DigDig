// 회복 증폭 - 힐로 회복되는 양 증가 (x1.3/1.5/1.8)
public class HealBoostAbility : Ability
{
    static readonly float[] multipliers = { 1.3f, 1.5f, 1.8f };
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"체력 회복 시 {(multipliers[c - 1] - 1) * 100}% 추가 회복";
    }
    public override void OnEquip(Player player)
    {
        player.healMultiplier *= multipliers[count - 1];
    }

    public override void OnUnequip(Player player)
    {
        player.healMultiplier /= multipliers[count - 1];
    }

    public override void UpdateEnhancement()
    {
        // count 변경 시 이전 배율 제거 후 새 배율 적용
        Player.Instance.healMultiplier /= multipliers[count - 2];
        Player.Instance.healMultiplier *= multipliers[count - 1];
    }
}
