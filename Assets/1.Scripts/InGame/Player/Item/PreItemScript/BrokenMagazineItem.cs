// 고장난 탄창: 탄창 수 -5
public class BrokenMagazineItem : Item
{
    Buff buff;

    public override void OnEquip()
    {
        // buff = new Buff(StatType.BulletCount, -5, StatOpType.Add);
        Player.Instance.AddBuff(buff);
    }

    public override void OnUnequip()
    {
        Player.Instance.RemoveBuff(buff);
    }
}
