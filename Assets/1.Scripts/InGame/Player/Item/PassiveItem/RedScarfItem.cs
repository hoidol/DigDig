public class RedScarfItem : Item
{
    Buff critBuff;
    bool pendingCrit;

    public override void OnEquip(Player player)
    {
        critBuff = new Buff(StatType.CritChance, 1f, StatOpType.Add); // CritChance 100% 추가
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFired);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFired);
        RemoveCritBuff();
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        pendingCrit = true;
        Player.Instance.AddBuff(critBuff);
    }

    void OnBulletFired(BulletFiredEvent e)
    {
        if (!pendingCrit) return;
        pendingCrit = false;
        RemoveCritBuff();
    }

    void RemoveCritBuff()
    {
        if (critBuff == null) return;
        Player.Instance.RemoveBuff(critBuff);
    }
}
