//30%>60%>100%
// 매끈한 노리쇠: 장전 속도 감소 20% > 25% > 30%
public class SmoothBoltItem : Item
{
    static readonly float[] reductions = { 0.85f, 0.7f, 0.50f };

    Buff buff;

    float Multiplier => reductions[UnityEngine.Mathf.Clamp(count - 1, 0, reductions.Length - 1)];

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.ReloadTime, Multiplier, StatOpType.Multiply);
        player.AddBuff(buff);
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.RemoveBuff(buff);
        buff = new Buff(StatType.ReloadTime, Multiplier, StatOpType.Multiply);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }
}
