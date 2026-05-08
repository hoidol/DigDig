// 가죽 장갑: 공격속도 증가 5% > 7% > 10%
public class LeatherGlovesItem : Item
{
    static readonly float[] bonuses = { 0.05f, 0.07f, 0.10f };

    Buff buff;

    float Bonus => bonuses[UnityEngine.Mathf.Clamp(count - 1, 0, bonuses.Length - 1)];

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackSpeed, 1f + Bonus, StatOpType.Divide);
        player.AddBuff(buff);
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.RemoveBuff(buff);
        buff = new Buff(StatType.AttackSpeed, 1f + Bonus, StatOpType.Divide);
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
        return $"공격속도 {bonus * 100:0}% 증가";
    }
}
