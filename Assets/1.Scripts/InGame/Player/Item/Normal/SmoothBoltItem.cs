// 매끈한 노리쇠: 장전 속도 40% 감소
public class SmoothBoltItem : Item
{
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.ReloadTime, 0.6f, StatOpType.Multiply);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }
}
