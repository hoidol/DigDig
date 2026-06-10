// 가죽 장갑: 공격속도 25% 증가
public class LeatherGlovesItem : Item
{
    Buff buff;
    const float buffValue =0.75f; 

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackSpeed, buffValue, StatOpType.Multiply);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
    {
        return "공격속도 25% 증가";
    }
}
