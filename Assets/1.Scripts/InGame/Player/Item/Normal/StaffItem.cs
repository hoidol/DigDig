// 지팡이: 마력 +6
public class StaffItem : Item
{
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.MagicPower, 6f, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
    {
        return "마력 +6";
    }
}
