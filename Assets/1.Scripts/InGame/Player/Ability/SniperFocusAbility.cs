// 저격 집중 - 이동하지 않으면 크리티컬 확률 대폭 증가
public class SniperFocusAbility : Ability
{
    static readonly float[] multipliers = { 1.2f, 1.25f, 1.3f };

    Buff buff;
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"이동하지 않으면 크리티컬 확률 {100 * (multipliers[c - 1] - 1)}% 상승";
    }

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.CritChance, multipliers[count - 1], StatOpType.Multiply);
    }

    public override void OnUnequip(Player player)
    {
        Player.Instance.RemoveBuff(buff);
    }

    public override void UpdateEnhancement()
    {
        if (buffApplied)
        {
            Player.Instance.RemoveBuff(buff);
            buff = new Buff(StatType.CritChance, multipliers[count - 1], StatOpType.Multiply);
            Player.Instance.AddBuff(buff);
        }

    }

    bool buffApplied;

    void Update()
    {
        bool stopped = Player.Instance.rg.linearVelocity.sqrMagnitude < 0.01f;
        if (stopped && !buffApplied)
        {
            Player.Instance.AddBuff(buff);
            buffApplied = true;
        }
        else if (!stopped && buffApplied)
        {
            Player.Instance.RemoveBuff(buff);
            buffApplied = false;
        }
    }
}
