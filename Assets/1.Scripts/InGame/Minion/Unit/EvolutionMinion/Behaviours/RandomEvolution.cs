using UnityEngine;

// 뭉치 (2026-11-26) 발동 스킬(머리 위 버튼): duration초 동안 공속 × skillAttackSpeedMul + 공격마다 부채꼴 fanCount발, 탄마다 피해 × fanDamageMul
// 적이 없어도 발동 (2026-11-26). 수치 = MinionEvolutionSetting.random
public class RandomEvolution : ActiveSkillEvolution
{
    RandomEvolutionConfig Cfg => Config<RandomEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.cooldown : 10f;
    protected override float Duration => Cfg != null ? Cfg.duration : 3f;
    bool spawningExtra;
    protected override Color SkillColor => new Color(1f, 0.85f, 0.3f); // 노랑 (속사)
    Buff buff;

    protected override bool TryStart()
    {
        RandomEvolutionConfig cfg = Cfg;
        if (cfg == null)
            return false;
        buff = new Buff(StatType.AttackSpeed, cfg.skillAttackSpeedMul, StatOpType.Multiply);
        minion.AddBuff(buff);
        minion.PlayPowerUpEffect(Duration);
        return true;
    }

    protected override void End()
    {
        if (buff != null && minion != null)
        {
            minion.RemoveBuff(buff);
            minion.StopPowerUpEffect();
        }
        buff = null;
    }

    public override void OnBulletFired(AllyBulletObject bullet)
    {
        RandomEvolutionConfig cfg = Cfg;
        if (spawningExtra || cfg == null || !IsActiveSkillRunning || cfg.fanCount <= 1)
            return;
        bullet.damageMultiplier = cfg.fanDamageMul; // Shoot 뒤라 덮어써도 됨
        spawningExtra = true;
        int extra = cfg.fanCount - 1;
        for (int i = 0; i < extra; i++)
        {
            // 원래 탄을 가운데로 좌우 번갈아: +1, -1, +2, -2 …
            int step = i / 2 + 1;
            float angle = (i % 2 == 0 ? 1 : -1) * step * cfg.fanAngle;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * bullet.shootDir;
            AllyBulletObject b = bullet.spec.Instantiate(bullet.allyUnit);
            b.transform.position = bullet.shootPos;
            b.Shoot(dir, bullet.shootDamage);
            b.damageMultiplier = cfg.fanDamageMul;
        }
        spawningExtra = false;
    }
}
