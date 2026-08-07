// 가죽 장화: 이동속도 25% 증가
public class LeatherBootsItem : Item
{
    Buff buff;
    float[] moveSpeedBonus = {1.25f, 1.3f,1.4f};
    public override void UpdateItem()
    {
        if(buff != null)
            Character.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.MoveSpeed,moveSpeedBonus[count-1] , StatOpType.Multiply);
        Character.Instance.AddBuff(buff);
    }

    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }
    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"이동속도 {moveSpeedBonus[lv-1]}% 증가";
    }
}
