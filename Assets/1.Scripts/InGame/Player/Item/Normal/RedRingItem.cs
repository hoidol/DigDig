// 붉은 링: 크리티컬 데미지 +100% (×2 → ×3)
public class RedRingItem : Item
{
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.CritPower, 1.0f, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
    {
        return "크리티컬 데미지 ×2 → ×3";
    }
}
