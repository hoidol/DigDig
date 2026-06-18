// 붉은 링: 크리티컬 데미지 +100% (×2 → ×3)
public class RedRingItem : Item
{
    Buff buff;
    float[] buffValues ={1.0f,1.2f,1.5f}; 
    public override void UpdateItem()
    {
        if(buff != null)
            Player.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.CritPower, buffValues[GetLevel()-1], StatOpType.Add);
        Player.Instance.AddBuff(buff);
    }    

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"크리티컬 데미지 {buffValues[lv-1]*100}% 증가";
    }
}
