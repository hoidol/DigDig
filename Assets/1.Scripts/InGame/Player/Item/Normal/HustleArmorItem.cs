// 허슬한 갑옷: 장착 개수에 따라 최대 체력 증가 30 > 70 > 120
public class HustleArmorItem : Item
{
    static readonly float[] bonuses = { 30f, 70f, 120f };

    Buff buff;

    float Bonus => bonuses[UnityEngine.Mathf.Clamp(count - 1, 0, bonuses.Length - 1)];

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.MaxHp, Bonus, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.RemoveBuff(buff);
        buff = new Buff(StatType.MaxHp, Bonus, StatOpType.Add);
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
        return $"최대 체력 +{bonus}";
    }
}
