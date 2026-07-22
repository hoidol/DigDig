// 붉은 눈: 크리티컬 확률 +10%
public class RedEyeItem : Item
{
    Buff buff;
    float[] buffValues ={10f,15,20f}; 
    public override void UpdateItem()
    {
        if(buff != null)
            Player.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.CritChance, buffValues[count-1], StatOpType.Add);
        Player.Instance.AddBuff(buff);
    }
    public override void OnUnequip()
    {
        Player.Instance.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"크리티컬 확률 +{buffValues[lv-1]}%";
    }
}
