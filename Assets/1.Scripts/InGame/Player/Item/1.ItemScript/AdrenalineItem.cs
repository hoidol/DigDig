// 혈투 - HP 30% 이하 시 공격력 +30%
public class AdrenalineItem : Item
{
    float[] HP_THRESHOLDS = { 0.4f, 0.5f, 0.6f };
    float[] BONUSES = { 0.3f, 0.4f, 0.5f };

    Buff buff;

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"HP {HP_THRESHOLDS[lv - 1] * 100}% 이하 시 공격력 {BONUSES[lv - 1] * 100}% 증가";
    }
    public override void UpdateItem()
    {
        if (buff != null)
            Player.Instance.RemoveBuff(buff);

        buff = new Buff(StatType.AttackPower, 1f + BONUSES[GetLevel()], StatOpType.Multiply);
    }

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        GameEventBus.Subscribe<PlayerHpChangedEvent>(OnHpChanged);
    }

    public override void OnUnequip(Player player)
    {
        if (buff != null)
        {
            player.RemoveBuff(buff);
            buff = null;
        }

        GameEventBus.Unsubscribe<PlayerHpChangedEvent>(OnHpChanged);
    }

    void OnHpChanged(PlayerHpChangedEvent e)
    {
        bool isLowHp = Player.Instance.curHp / Player.Instance.statMgr.MaxHp <= HP_THRESHOLDS[GetLevel() - 1];
        if (isLowHp)
            Player.Instance.AddBuff(buff);
        else
            Player.Instance.RemoveBuff(buff);
    }
}
