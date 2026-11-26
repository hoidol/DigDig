using UnityEngine;

// 늑돌이 (2026-11-26) 발동 스킬: duration초 동안 interval초마다 공격 범위 안 적 전부를 할큄 (공격력 × damageMul + 감속)
// 적이 없어도 발동 (2026-11-26). 수치 = MinionEvolutionSetting.berserker
public class BerserkerEvolution : ActiveSkillEvolution
{
    BerserkerEvolutionConfig Cfg => Config<BerserkerEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.cooldown : 10f;
    protected override float Duration => Cfg != null ? Cfg.duration : 4f;
    float tickTimer;
    protected override Color SkillColor => Cfg != null ? Cfg.clawColor : new Color(1f, 0.55f, 0.45f);

    protected override bool TryStart()
    {
        tickTimer = 0f; // 바로 한 번
        minion.PlayPowerUpEffect(Duration);
        return true;
    }

    protected override void RunTick(float dt)
    {
        BerserkerEvolutionConfig cfg = Cfg;
        if (cfg == null)
            return;
        tickTimer -= dt;
        if (tickTimer > 0f)
            return;
        tickTimer = Mathf.Max(0.05f, cfg.interval);
        if (minion.IsBeingDragged)
            return;
        Vector2 center = minion.transform.position;
        float damage = minion.AttackPower * cfg.damageMul;
        bool hit = false;
        foreach (IHittable e in FindEnemies(minion, center, minion.AttackRange))
        {
            Vector2 pos = e.Transform.position;
            HeroSkillVisual.PlayClaw(pos, (pos - center).normalized, cfg.clawColor, 0.9f, 0.08f, 0.22f);
            DealDamage(minion, e, damage);
            if (cfg.slowDuration > 0f)
                ApplyStatus(e, new SlowEffect(cfg.slowDuration));
            hit = true;
        }
        if (hit)
            SoundManager.Play(SFXType.BerserkerHit);
    }

    protected override void End()
    {
        if (minion != null)
            minion.StopPowerUpEffect();
    }
}
