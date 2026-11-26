using UnityEngine;

// 콩콩이 (2026-11-26): 탄이 맞으면 확률 스턴 (항상)
// + 발동 스킬(머리 위 버튼, 쿨타임 buffCooldown): buffDuration초 동안 공속 × buffAttackSpeedMul. 적이 없어도 발동
// 수치 = MinionEvolutionSetting.bounce
public class BounceEvolution : ActiveSkillEvolution
{
    BounceEvolutionConfig Cfg => Config<BounceEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.buffCooldown : 7f;
    protected override float Duration => Cfg != null ? Cfg.buffDuration : 4f;
    Buff buff;
    protected override Color SkillColor => new Color(0.5f, 1f, 0.55f); // 초록 (공속 버프)

    public override void OnBulletFired(AllyBulletObject bullet)
    {
        BounceEvolutionConfig cfg = Cfg;
        if (cfg != null && cfg.stunChance > 0f)
            bullet.AddBehavior(new StunOnHitBehavior(cfg.stunDuration, cfg.stunChance));
    }

    protected override bool TryStart()
    {
        BounceEvolutionConfig cfg = Cfg;
        if (cfg == null)
            return false;
        buff = new Buff(StatType.AttackSpeed, cfg.buffAttackSpeedMul, StatOpType.Multiply);
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
}
