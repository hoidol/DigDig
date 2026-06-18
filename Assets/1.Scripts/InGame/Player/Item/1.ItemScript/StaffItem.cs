// 지팡이: 탄 효율 15, 20, 25
public class StaffItem : Item
{
    Buff buff;

    float[] buffValues ={0.15f,0.20f,0.25f}; 

    public override void UpdateItem()
    {
        if(buff != null)
            Player.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.AttackPower, buffValues[GetLevel()-1], StatOpType.Add);
        Player.Instance.AddBuff(buff);
    }    

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"탄 효율 {buffValues[lv-1]*100}% 증가";
    }
}
