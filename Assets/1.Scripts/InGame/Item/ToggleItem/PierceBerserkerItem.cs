public class PierceBerserkerItem : Item, IToggleItem, IBulletItem
{
    const float HP_THRESHOLD = 0.3f;
    const float ATTACK_BONUS = 0.2f;

    Buff buff;
    bool isOn;

    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackPower, 1f + ATTACK_BONUS * count, StatOpType.Multiply);
    }

    public override void OnUnequip(Player player)
    {
        if (isOn) OnTurnOff();
    }

    public override void UpdateItem()
    {
        // count 변화 시 버프 세기 갱신
        bool wasOn = isOn;
        if (wasOn) OnTurnOff();
        buff = new Buff(StatType.AttackPower, 1f + ATTACK_BONUS * count, StatOpType.Multiply);
        if (wasOn) OnTurnOn();
    }

    void Update()
    {
        var player = Player.Instance;
        bool condition = CheckCondition();
        if (condition && !isOn) OnTurnOn();
        else if (!condition && isOn) OnTurnOff();
    }

    public bool CheckCondition()
    {
        return Player.Instance.curHp / Player.Instance.playerStatMgr.MaxHp < HP_THRESHOLD;
    }

    public void OnTurnOn()
    {
        isOn = true;
        Player.Instance.playerStatMgr.AddBuff(buff);
    }

    public void OnTurnOff()
    {
        isOn = false;
        Player.Instance.playerStatMgr.RemoveBuff(buff);
    }
    public int pierceCount = 2;

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new PierceBehavior(pierceCount));
    }
}
