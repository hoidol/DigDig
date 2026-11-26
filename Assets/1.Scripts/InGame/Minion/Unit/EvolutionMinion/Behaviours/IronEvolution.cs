using UnityEngine;

// 철컹이 (2026-11-26): 엘리트·보스 피해 배율 = eliteBossDamageMul (IronMinion.GetBullet이 읽음)
// + 보스를 때리면 주변 적(보스 제외)에게 보스에게 준 피해 × bossSplashRatio
// 수치 = MinionEvolutionSetting.iron
public class IronEvolution : MinionEvolution
{
    IronEvolutionConfig Cfg => Config<IronEvolutionConfig>();
    public float EliteBossDamageMul => Cfg != null ? Cfg.eliteBossDamageMul : 0f;

    public override void OnBulletFired(AllyBulletObject bullet)
    {
        bullet.AddBehavior(new BossSplash(this));
    }

    void Splash(BulletObject bullet, IHittable hit)
    {
        IronEvolutionConfig cfg = Cfg;
        if (cfg == null || minion == null || !(hit is Enemy enemy))
            return;
        bool isBoss = enemy is Boss;
        if (!isBoss && !(cfg.splashOnElite && enemy.IsEliteOrBoss))
            return;
        float dealt = bullet is AllyBulletObject ab && ab.DamageData != null ? ab.DamageData.damage : minion.AttackPower;
        float damage = dealt * cfg.bossSplashRatio;
        if (damage <= 0f)
            return;
        Vector2 center = hit.Transform.position;
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.SmallExplosion, center);
        HeroSkillVisual.PlayRing(center, cfg.bossSplashRadius, new Color(0.85f, 0.9f, 1f), 0.3f, 0.09f); // (2026-11-26) 스플래시 범위
        foreach (IHittable e in FindEnemies(minion, center, cfg.bossSplashRadius))
            if (e != hit)
                DealDamage(minion, e, damage);
    }

    class BossSplash : IBulletBehavior
    {
        readonly IronEvolution owner;
        public BossSplash(IronEvolution owner) { this.owner = owner; }
        public int ApplyOrder => 0;
        public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
        {
            if (owner != null)
                owner.Splash(bullet, hit);
            return true;
        }
    }
}
