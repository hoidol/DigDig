// 지팡이: 마력 +4 > +7 > +10
public class StaffItem : Item
{
    static readonly float[] bonuses = { 4f, 7f, 10f };

    Buff buff;

    float Bonus => bonuses[UnityEngine.Mathf.Clamp(count - 1, 0, bonuses.Length - 1)];

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.MagicPower, Bonus, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.RemoveBuff(buff);
        buff = new Buff(StatType.MagicPower, Bonus, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        float bonus = bonuses[UnityEngine.Mathf.Clamp(c - 1, 0, bonuses.Length - 1)];
        return $"마력 +{bonus}";
    }
}
