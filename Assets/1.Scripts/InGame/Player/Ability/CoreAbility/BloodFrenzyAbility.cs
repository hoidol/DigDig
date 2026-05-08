// 혈투 - HP 30% 이하 시 공격력 +20~60%
public class BloodFrenzyAbility : Ability
{
    Buff buff;
    const float HP_THRESHOLD = 0.3f;
    float[] bonuses = { 0.2f, 0.3f, 0.4f };
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"체력{HP_THRESHOLD * 100}% 이하일때 공격력이 {bonuses[c - 1] * 100}% 증가";
    }
    public override void OnEquip(Player player)
    {

        GameEventBus.Subscribe<PlayerHpChangedEvent>(OnHpChanged);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
        GameEventBus.Unsubscribe<PlayerHpChangedEvent>(OnHpChanged);
    }

    void OnHpChanged(PlayerHpChangedEvent e) { UpdateAbility(); }

    public override void UpdateEnhancement()
    {
        float bonus = bonuses[count - 1]; // 1중첩:20%, 2:30%, 3:50%
        buff = new Buff(StatType.AttackPower, 1f + bonus, StatOpType.Multiply);
    }

    void UpdateAbility()
    {

        bool isLowHp = Player.Instance.curHp / Player.Instance.statMgr.MaxHp <= HP_THRESHOLD;
        if (isLowHp)
            Player.Instance.AddBuff(buff);
        else
            Player.Instance.RemoveBuff(buff);
    }
}
