// 가죽 장화: 이동속도 25% 증가
public class LeatherBootsItem : Item
{
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.MoveSpeed, 1.25f, StatOpType.Multiply);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
    {
        return "이동속도 25% 증가";
    }
}
