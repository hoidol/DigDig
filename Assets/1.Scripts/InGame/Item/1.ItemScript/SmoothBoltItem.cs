// 매끈한 노리쇠: 장전 속도 40% 감소
public class SmoothBoltItem : Item
{
    Buff buff;
    float[] buffValues ={1.3f,1.4f,1.5f}; 
    public override void UpdateItem()
    {
        if(buff != null)
            Character.Instance.RemoveBuff(buff);
            
        // buff = new Buff(StatType.ReloadSpeed, buffValues[count-1], StatOpType.Multiply);
        Character.Instance.AddBuff(buff);
    }    


    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"장전 속도 {(buffValues[lv-1]-1) * 100}% 증가";
    }
}
