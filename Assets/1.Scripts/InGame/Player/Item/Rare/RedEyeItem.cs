// [붉은 눈]
// 크리티컬 확률을 count에 따라 증가시키는 패시브 아이템.
// Buff(StatType.CritChance, Add)를 플레이어에 적용하며, count 변경 시 기존 버프를 교체.
public class RedEyeItem : Item
{
    Buff buff;
    public float[] addCritChance = { 10f, 20f, 30f };

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.CritChance, addCritChance[count - 1], StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.RemoveBuff(buff);
        buff = new Buff(StatType.CritChance, addCritChance[count - 1], StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0)
            return $"크리티컬 확률 {addCritChance[0]}/{addCritChance[1]}/{addCritChance[2]}% 증가";
        else
            return $"크리티컬 확률 {addCritChance[c - 1]}% 증가";
    }
}
