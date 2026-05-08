// 짙은 피 냄새 - 적 처치 후 다음 공격 크리티컬 확률 대폭 상승 (30%/40%/50%)
public class BloodScentAbility : Ability, IBulletItem
{
    static readonly float[] bonusCritChances = { 30f, 40f, 50f };

    bool applyNext;

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"적 처치 시 크리티컬 확률 {bonusCritChances[c - 1]}% 상승";
    }

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        applyNext = false;
    }

    void OnEnemyDead(EnemyDeadEvent e) => applyNext = true;

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (!applyNext) return;
        applyNext = false;
        bullet.AddBulletForce(new BonusCritChanceBehavior(bonusCritChances[count - 1]));
    }
}
