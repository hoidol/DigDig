// 붕대: 30초마다 체력 5 회복
public class BandageItem : TriggerItem
{
    public override void OnEquip(Player player)
    {
        coolTime = 30f;
        base.OnEquip(player);
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        Player.Instance.AddHp(5f);
    }

    public override string GetDescription(bool detail = false)
    {
        return $"{coolTime}초 마다 5만큼 체력 회복";
    }
}
