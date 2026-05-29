// 혈투 - HP 30% 이하 시 공격력 +40%
public class BloodFrenzyAbility : Ability
{
    const float HP_THRESHOLD = 0.3f;
    const float BONUS = 0.4f;

    Buff buff;

    public override string GetDescription(bool detail = false)
    {
        return $"HP {HP_THRESHOLD * 100}% 이하 시 공격력 {BONUS * 100}% 증가";
    }

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackPower, 1f + BONUS, StatOpType.Multiply);
        GameEventBus.Subscribe<PlayerHpChangedEvent>(OnHpChanged);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
        GameEventBus.Unsubscribe<PlayerHpChangedEvent>(OnHpChanged);
    }

    void OnHpChanged(PlayerHpChangedEvent e)
    {
        bool isLowHp = Player.Instance.curHp / Player.Instance.statMgr.MaxHp <= HP_THRESHOLD;
        if (isLowHp)
            Player.Instance.AddBuff(buff);
        else
            Player.Instance.RemoveBuff(buff);
    }
}
