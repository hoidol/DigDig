using UnityEngine;

// 불똥이 → 마그미 (2026-11-26): 탄이 맞은 자리에 마그마 스플래시 — 주변 적 피해(공격력 × splashDamageMul) + 화상, 맞은 적·스플래시 적마다 확률 스턴
// 수치 = MinionEvolutionSetting.flame
public class FlameEvolution : MinionEvolution
{
    FlameEvolutionConfig Cfg => Config<FlameEvolutionConfig>();
    static readonly Color MAGMA_COLOR = new Color(1f, 0.45f, 0.15f);

    public override void OnBulletFired(AllyBulletObject bullet)
    {
        bullet.AddBehavior(new MagmaSplash(this));
    }

    void Splash(IHittable hit)
    {
        FlameEvolutionConfig cfg = Cfg;
        if (cfg == null || minion == null || hit == null)
            return;
        Vector2 center = hit.Transform.position;
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.SmallExplosion, center);
        HeroSkillVisual.PlayRing(center, cfg.splashRadius, MAGMA_COLOR, 0.25f, 0.07f); // (2026-11-26) 마그마 스플래시 범위
        TryStun(hit, cfg.stunChance, cfg.stunDuration);
        FlameMinion flame = minion as FlameMinion;
        float damage = minion.AttackPower * cfg.splashDamageMul;
        foreach (IHittable e in FindEnemies(minion, center, cfg.splashRadius))
        {
            if (e == hit)
                continue;
            DealDamage(minion, e, damage, AttackType.Magic);
            if (cfg.splashBurn && flame != null)
                ApplyStatus(e, new FlameEffect(flame.burnDuration, flame.burnDPS, minion));
            TryStun(e, cfg.stunChance, cfg.stunDuration);
        }
    }

    class MagmaSplash : IBulletBehavior
    {
        readonly FlameEvolution owner;
        public MagmaSplash(FlameEvolution owner) { this.owner = owner; }
        public int ApplyOrder => 0;
        public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
        {
            if (owner != null)
                owner.Splash(hit);
            return true;
        }
    }
}
