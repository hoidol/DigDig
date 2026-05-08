// 고장난 탄창: 탄창 수 -2
public class BrokenMagazineItem : Item
{
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.BulletCount, -2 * count, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.RemoveBuff(buff);
        buff = new Buff(StatType.BulletCount, -2 * count, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }
}
