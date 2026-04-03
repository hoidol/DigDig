public class RedEyeItem : Item
{
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.CritChance, 0.1f * count, StatOpType.Add);
        player.playerStatMgr.AddBuff(buff);
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.playerStatMgr.RemoveBuff(buff);
        buff = new Buff(StatType.CritChance, 0.1f * count, StatOpType.Add);
        player.playerStatMgr.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.playerStatMgr.RemoveBuff(buff);
    }
}
