// 붉은 눈: 크리티컬 확률 +15%
public class RedEyeItem : Item
{
    Buff buff;
    const float buffValue =15f; 

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.CritChance, buffValue, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
    {
        return "크리티컬 확률 +15%";
    }
}
