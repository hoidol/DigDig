// 정신력 물약: 마력 +1
public class WisdomPotionItem : Item
{
    const float BONUS = 1f;
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.MagicPower, BONUS, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void UpdateItem() { }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int c = -1, bool detail = false)
        => $"마력 +{BONUS}";
}
