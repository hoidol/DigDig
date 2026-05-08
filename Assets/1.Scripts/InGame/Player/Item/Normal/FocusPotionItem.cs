// 집중력 물약: 공격력 +2
public class FocusPotionItem : Item
{
    const float BONUS = 2f;
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackPower, BONUS, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void UpdateItem() { }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int c = -1, bool detail = false)
        => $"공격력 +{BONUS}";
}
