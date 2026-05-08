// 체력 물약: 최대 체력 +10
public class VitalityPotionItem : Item
{
    const float BONUS = 10f;
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.MaxHp, BONUS, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void UpdateItem() { }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int c = -1, bool detail = false)
        => $"체력 +{BONUS}";
}
