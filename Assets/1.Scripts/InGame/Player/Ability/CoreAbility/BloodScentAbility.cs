// 짙은 피 냄새 - 적 처치 시 다음 공격 크리티컬 확률 30% 상승
public class BloodScentAbility : Ability, IBullet
{
    const float BONUS_CRIT = 30f;

    bool applyNext;

    public override string GetDescription(bool detail = false)
    {
        return $"적 처치 시 크리티컬 확률 {BONUS_CRIT}% 상승";
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

    public void OnBulletFired(PlayerBulletObject bullet)
    {
        if (!applyNext) return;
        applyNext = false;
        bullet.AddBulletForce(new BonusCritChanceBehavior(BONUS_CRIT));
    }
}
