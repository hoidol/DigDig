// 고장난 탄창: 탄창 수 -5
public class BrokenMagazineItem : Item
{
    Buff buff;

    public override void OnEquip()
    {
        // buff = new Buff(StatType.BulletCount, -5, StatOpType.Add);
        Character.Instance.AddBuff(buff);
    }

    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }
}
