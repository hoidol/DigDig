// 가죽 장화: 이동속도 25% 증가
public class LeatherBootsItem : Item
{
    Buff buff;
    float[] moveSpeedBonus = {1.25f, 1.3f,1.4f};
    public override void UpdateItem()
    {
        if(buff != null)
            Player.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.MoveSpeed,moveSpeedBonus[GetLevel()-1] , StatOpType.Multiply);
        Player.Instance.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }
    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"이동속도 {moveSpeedBonus[lv-1]}% 증가";
    }
}
