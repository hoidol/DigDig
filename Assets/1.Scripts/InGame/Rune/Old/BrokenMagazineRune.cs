// 고장난 탄창: 탄창 수 -5
public class BrokenMagazineRune : Rune
{
    Buff buff;

    public override void OnEquip()
    {
        // buff = new Buff(StatType.BulletCount, -5, StatOpType.Add);
        Slime.Instance.AddBuff(buff);
    }

    public override void OnUnequip()
    {
        Slime.Instance.RemoveBuff(buff);
    }
}
