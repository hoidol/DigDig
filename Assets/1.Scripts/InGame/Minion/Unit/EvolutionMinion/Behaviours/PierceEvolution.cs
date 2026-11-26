using UnityEngine;

// 뾰족이 (2026-11-26): 탄이 화면 끝에 닿으면 반사 (항상, 최대 maxScreenBounce번, 반사마다 관통 횟수 1 소모 → 다 쓰면 사라짐)
// + 발동 스킬(머리 위 버튼, 쿨타임 skillCooldown): skillBulletCount방향 원형 관통탄 (첫 방향 = 가장 가까운 적, 적이 없어도 발동 = 위쪽부터)
// 수치 = MinionEvolutionSetting.pierce
public class PierceEvolution : ActiveSkillEvolution
{
    PierceEvolutionConfig Cfg => Config<PierceEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.skillCooldown : 10f;
    protected override float Duration => 0.2f; // 즉시 발사 (버튼이 잠깐 잠기는 시간)
    protected override Color SkillColor => new Color(0.75f, 0.95f, 1f); // 흰 하늘색

    public override void OnBulletFired(AllyBulletObject bullet)
    {
        PierceEvolutionConfig cfg = Cfg;
        if (cfg == null)
            return;
        bullet.screenBounceLeft = cfg.maxScreenBounce;
        bullet.screenBounceConsumesPierce = cfg.bounceConsumesPierce;
    }

    protected override bool TryStart()
    {
        PierceEvolutionConfig cfg = Cfg;
        if (cfg == null)
            return false;
        Transform target = minion.FindClosestTargetInRange();
        int count = Mathf.Max(1, cfg.skillBulletCount);
        if (target != null)
            minion.FireCircleAt(target, count);
        else
            minion.FireCircle(90f, count); // 적이 없으면 위쪽부터
        return true;
    }
}
