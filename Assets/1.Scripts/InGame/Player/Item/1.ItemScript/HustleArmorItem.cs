// 허슬한 갑옷: 최대 체력 +80
public class HustleArmorItem : Item
{
    Buff buff;
    float[] bonusHps = {80f, 110f,150f};
    public override void UpdateItem()
    {
        if(buff != null)
            Player.Instance.RemoveBuff(buff);
            
        buff = new Buff(StatType.MaxHp, bonusHps[GetLevel()-1], StatOpType.Add);
        Player.Instance.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"최대 체력 +{bonusHps[lv-1]}";
    }
}
