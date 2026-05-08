//시간 마다 체력 회복
public class BandageItem : TriggerItem
{
    public float[] healAmounts = { 5f, 7f, 9f };

    public override void OnTrigger()
    {
        base.OnTrigger();
        Player.Instance.AddHp(healAmounts[count - 1]);
    }
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{coolTime}초 마다 {healAmounts[c - 1]}만큼 체력 회복";
    }
}
