using System.Linq;
using UnityEngine;

// 진흙 구슬: 회전하다 적에 닿으면 터짐 → 주변(splashRadius) 적 최대 maxHitCount기에게 피해 + 진흙 1중첩, 구슬은 사라짐
// 피해/타일 효과는 OrbitOrb.OnHit 그대로 (스플래시 대상마다 호출)
public class MudOrb : OrbitOrb
{
    bool popped; // 같은 프레임에 여러 적과 닿아도 한 번만 터짐

    void OnEnable()
    {
        popped = false;
    }

    public override void OnHit(Collider2D other, IHittable hittable)
    {
        if (popped)
            return;
        MudSlime mud = orbitMachine.owner as MudSlime;
        if (mud == null)
            return;
        popped = true;

        Vector2 center = transform.position;
        EffectManager.Instance.Play(EffectType.SmallExplosion, center);

        IHittable[] targets = Physics2D.OverlapCircleAll(center, mud.splashRadius, layerMask)
            .Select(c => c.GetComponent<IHittable>())
            .Where(h => h != null)
            .OrderBy(h => Vector2.Distance(center, h.Transform.position))
            .Take(mud.maxHitCount)
            .ToArray();

        foreach (IHittable target in targets)
        {
            base.OnHit(null, target);
            if (target is Enemy enemy && enemy.CurHp > 0)
                enemy.ApplyStatusEffect(new MudEffect(mud.mudDuration));
        }

        orbitMachine.RemoveOrbitBullet(this);
    }
}
