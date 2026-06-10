// 지팡이: 마력 +6
public class StaffItem : Item
{
    Buff buff;

    const float buffValue =0.15f; 
    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackPower, buffValue, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
    {
        return $"탄 효율 {buffValue*100}% 증가";
    }
}
