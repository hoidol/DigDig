// 집중 대응 - 이동하지 않으면 공격 속도 증가
public class StillFocusAbility : Ability
{
    static readonly float[] multipliers = { 1.3f, 1.6f, 2.0f };

    Buff buff;
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"이동하지 않으면 공격속도 {100 * (multipliers[c - 1] - 1)}% 상승";
    }
    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackSpeed, multipliers[count - 1], StatOpType.Multiply);

    }

    public override void OnUnequip(Player player)
    {
        Player.Instance.RemoveBuff(buff);
    }

    public override void UpdateEnhancement()
    {
        Player.Instance.RemoveBuff(buff);
        buff = new Buff(StatType.AttackSpeed, multipliers[count - 1], StatOpType.Multiply);
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
