// 즉시 회복 - 장착/강화 시 최대 체력의 20% 즉시 회복
public class InstantHealAbility : Ability
{
    const float HEAL_RATIO = 0.2f;
    public override string GetDescription(int c = -1, bool detail = false)
    {

        return $"최대 체력의 {HEAL_RATIO * 100}% 만큼 즉시 회복";
    }
    public override void OnEquip(Player player)
    {
        Heal();
    }

    public override void OnUnequip(Player player) { }

    public override void LevelUp()
    {
        Heal();
    }
    void Heal()
    {

        Player.Instance.AddHp(Player.Instance.statMgr.MaxHp * HEAL_RATIO);
    }
}
