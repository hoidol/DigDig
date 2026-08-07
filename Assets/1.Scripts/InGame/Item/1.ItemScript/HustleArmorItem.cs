// 허슬한 갑옷: 최대 체력 +80
public class HustleArmorItem : Item
{
    Buff buff;
    float[] bonusHps = {80f, 110f,150f};
    public override void UpdateItem()
    {
        if(buff != null)
            Character.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.MaxHp, bonusHps[count-1], StatOpType.Add);
        Character.Instance.AddBuff(buff);
    }

    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"최대 체력 +{bonusHps[lv-1]}";
    }
}
