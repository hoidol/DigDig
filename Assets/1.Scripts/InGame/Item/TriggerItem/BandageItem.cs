//시간 마다 체력 회복
public class BandageItem : TriggerItem
{
    public float healAmount = 5f;

    public override void OnTrigger()
    {
        Player.Instance.AddHp(healAmount * count);
    }
}
