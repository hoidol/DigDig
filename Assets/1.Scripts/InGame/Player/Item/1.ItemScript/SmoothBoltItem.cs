// 매끈한 노리쇠: 장전 속도 40% 감소
public class SmoothBoltItem : Item
{
    Buff buff;
    const float buffValue =1.3f; 

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.ReloadSpeed, buffValue, StatOpType.Multiply);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }
}
