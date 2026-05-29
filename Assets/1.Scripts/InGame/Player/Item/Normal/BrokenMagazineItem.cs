// 고장난 탄창: 탄창 수 -5
public class BrokenMagazineItem : Item
{
    Buff buff;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.BulletCount, -5, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }
}
