// 지팡이: 탄 효율 15, 20, 25
public class StaffItem : Item
{
    Buff buff;

    float[] buffValues ={0.15f,0.20f,0.25f}; 

    public override void UpdateItem()
    {
        if(buff != null)
            Character.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.AttackPower, buffValues[count-1], StatOpType.Add);
        Character.Instance.AddBuff(buff);
    }    

    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"탄 효율 {buffValues[lv-1]*100}% 증가";
    }
}
