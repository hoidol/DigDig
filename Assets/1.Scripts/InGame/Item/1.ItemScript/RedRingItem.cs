// 붉은 링: 크리티컬 데미지 +100% (×2 → ×3)
public class RedRingItem : Item
{
    Buff buff;
    float[] buffValues ={1.0f,1.2f,1.5f}; 
    public override void UpdateItem()
    {
        if(buff != null)
            Character.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.CritPower, buffValues[count-1], StatOpType.Add);
        Character.Instance.AddBuff(buff);
    }    

    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"크리티컬 데미지 {buffValues[lv-1]*100}% 증가";
    }
}
