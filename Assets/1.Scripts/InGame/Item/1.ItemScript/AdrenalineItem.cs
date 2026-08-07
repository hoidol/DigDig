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
            Character.Instance.RemoveBuff(buff);

        buff = new Buff(StatType.AttackPower, 1f + BONUSES[count], StatOpType.Multiply);
    }

    public override void OnEquip()
    {
        base.OnEquip();
        GameEventBus.Subscribe<CharacterHpChangedEvent>(OnHpChanged);
    }

    public override void OnUnequip()
    {
        if (buff != null)
        {
            Character.Instance.RemoveBuff(buff);
            buff = null;
        }

        GameEventBus.Unsubscribe<CharacterHpChangedEvent>(OnHpChanged);
    }

    void OnHpChanged(CharacterHpChangedEvent e)
    {
        bool isLowHp = Character.Instance.curHp / Character.Instance.statMgr.MaxHp <= HP_THRESHOLDS[count - 1];
        if (isLowHp)
            Character.Instance.AddBuff(buff);
        else
            Character.Instance.RemoveBuff(buff);
    }
}
