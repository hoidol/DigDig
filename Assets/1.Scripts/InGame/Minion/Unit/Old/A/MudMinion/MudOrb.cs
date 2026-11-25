using System.Linq;
using UnityEngine;

// 진흙 구슬: 회전하다 적에 닿으면 주변(splashRadius) 적 최대 maxHitCount기에게 피해 + 진흙 1중첩
// 구슬은 사라지지 않고 계속 회전 (2026-11-16, 이전: 터지면 사라짐)
// 같은 적은 HIT_COOLDOWN(0.5초) 동안 이 구슬에 다시 안 맞음 (닿은 적·스플래시로 맞은 적 모두)
// 피해/타일 효과는 OrbitOrb.OnHit 그대로 (스플래시 대상마다 호출)
public class MudOrb : OrbitOrb
{
    public override void OnHit(Collider2D other, IHittable hittable)
    {
        MudMinion mud = orbitMachine.owner as MudMinion;
        if (mud == null)
            return;

        Vector2 center = transform.position;
        EffectManager.Instance.Play(EffectType.SmallExplosion, center);

        IHittable[] targets = Physics2D.OverlapCircleAll(center, mud.splashRadius, layerMask)
            .Select(c => c.GetComponent<IHittable>())
            .Where(h => h != null && (h == hittable || !hitCooldowns.Exists(cd => cd.hittable == h)))
            .OrderBy(h => Vector2.Distance(center, h.Transform.position))
            .Take(mud.maxHitCount)
            .ToArray();

        foreach (IHittable target in targets)
        {
            if (target != hittable)
                hitCooldowns.Add(new HitCooldown { hittable = target, cooltime = HIT_COOLDOWN });
            base.OnHit(null, target);
            if (target is Enemy enemy && enemy.CurHp > 0)
                enemy.ApplyStatusEffect(new MudEffect(mud.mudDuration));
        }
    }
}
