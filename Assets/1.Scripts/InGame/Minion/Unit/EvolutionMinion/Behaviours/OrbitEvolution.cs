using UnityEngine;

// 빙빙이 (2026-11-26) 발동 스킬: duration초 동안 회전체가 닿은 적 주변에 스플래시 피해(회전체 피해 × splashDamageMul) + 감속 (맞은 적 포함)
// 수치 = MinionEvolutionSetting.orbit
public class OrbitEvolution : ActiveSkillEvolution
{
    OrbitEvolutionConfig Cfg => Config<OrbitEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.cooldown : 10f;
    protected override float Duration => Cfg != null ? Cfg.duration : 5f;
    SplashSlow behaviour;
    protected override Color SkillColor => new Color(0.8f, 0.6f, 1f); // 보라

    protected override bool TryStart()
    {
        if (!(minion is OrbitMinion orbit) || orbit.orbitMachine == null)
            return false;
        behaviour = new SplashSlow(this);
        foreach (OrbitOrb orb in orbit.orbitMachine.orbitOrbs)
            if (orb != null)
                orb.AddBulletBehaviour(behaviour);
        minion.PlayPowerUpEffect(Duration);
        return true;
    }

    protected override void End()
    {
        if (minion is OrbitMinion orbit && orbit.orbitMachine != null && behaviour != null)
            foreach (OrbitOrb orb in orbit.orbitMachine.orbitOrbs)
                if (orb != null)
                    orb.bulletBehaviors.Remove(behaviour);
        behaviour = null;
        if (minion != null)
            minion.StopPowerUpEffect();
    }

    void Splash(IHittable hit)
    {
        OrbitEvolutionConfig cfg = Cfg;
        if (cfg == null || minion == null || !(minion is OrbitMinion orbit))
            return;
        Vector2 center = hit.Transform.position;
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.Hit, center);
        HeroSkillVisual.PlayRing(center, cfg.splashRadius, SkillColor, 0.25f, 0.06f); // 스플래시 범위
        float damage = orbit.orbitMachine.damage * cfg.splashDamageMul;
        foreach (IHittable e in FindEnemies(minion, center, cfg.splashRadius))
        {
            if (e != hit)
                DealDamage(minion, e, damage);
            if (cfg.slowDuration > 0f)
                ApplyStatus(e, new SlowEffect(cfg.slowDuration));
        }
    }

    class SplashSlow : IBulletBehavior
    {
        readonly OrbitEvolution owner;
        public SplashSlow(OrbitEvolution owner) { this.owner = owner; }
        public int ApplyOrder => 0;
        public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
        {
            if (owner != null && hit != null)
                owner.Splash(hit);
            return true;
        }
    }
}
