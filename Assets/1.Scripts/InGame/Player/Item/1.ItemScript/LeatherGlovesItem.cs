// 가죽 장갑: 공격속도 25% 증가
public class LeatherGlovesItem : Item
{
    Buff buff;
    const float buffValue =0.75f; 

    float[] buffValues = {0.8f, 0.7f, 0.6f};
    public override void UpdateItem()
    {
        if(buff != null)
            Player.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.AttackSpeed,buffValues[GetLevel()-1] , StatOpType.Multiply);
        Player.Instance.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"공격속도 {1-buffValues[lv-1]*100}% 증가";
    }
}
