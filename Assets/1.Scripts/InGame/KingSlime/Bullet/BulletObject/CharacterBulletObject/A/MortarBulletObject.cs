using UnityEngine;
using System.Linq;

// 폭격 슬라임 폭탄: 깃발(spec.targetPos)까지 포물선으로 날아가 착탄 지점에서 범위 폭발
// - 비행 중엔 충돌 판정 없음 (CheckHit 안 함)
// - 피해는 낙뢰(ThunderBulletObject)와 같은 방식: OnFired에서 붙은 타일 효과(치명타·만능·거인 사냥 / 둔화·스턴·처치 코인) 그대로 적용
// - 연발 타일은 같은 spec으로 다시 쏘므로 같은 깃발로 떨어짐
public class MortarBulletObject : AllyBulletObject
{
    MortarBulletSpec mortarSpec;
    Vector2 startPos;
    Vector2 targetPos;
    float flightTime;
    float arcHeight;
    float timer;

    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet, allyUnit);
        mortarSpec = bullet as MortarBulletSpec;
    }

    public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage);
        startPos = transform.position;
        targetPos = mortarSpec.targetPos;
        flightTime = Mathf.Max(0.01f, mortarSpec.flightTime);
        arcHeight = mortarSpec.arcHeight;
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / flightTime);
        Vector2 pos = Vector2.Lerp(startPos, targetPos, t);
        pos.y += arcHeight * 4f * t * (1f - t);
        transform.position = pos;

        if (t >= 1f)
            Explode();
    }

    void Explode()
    {
        EffectManager.Instance.Play(EffectType.Explosion, targetPos);

        IHittable[] targets = Physics2D.OverlapCircleAll(targetPos, mortarSpec.boomRange, mortarSpec.hitLayerMask)
            .Select(c => c.GetComponent<IHittable>())
            .Where(h => h != null)
            .OrderBy(h => Vector2.Distance(targetPos, h.Transform.position))
            .Take(mortarSpec.maxHitCount)
            .ToArray();

        foreach (IHittable hit in targets)
        {
            float finalDamage = damage * damageMultiplier;
            for (int j = 0; j < forces.Count; j++)
                finalDamage += forces[j].GetMultiDamage(this, hit, default, direction);
            if (finalDamage < 1f)
                finalDamage = 1f;

            // Init은 하지 않음: OnFired에서 넣은 타일 효과가 지워짐
            allyUnit.AccumulateDamage(finalDamage);
            allyUnitDamageData.damage = finalDamage;
            hit.TakeDamage(allyUnitDamageData);

            // 적중 후 효과 (슬로우 50%, 각성 넉백, 타일 효과). 넉백 방향 = 폭발 중심 → 적
            Vector2 pushDir = ((Vector2)hit.Transform.position - targetPos).normalized;
            foreach (var b in behaviors)
                b.OnHit(this, hit, default, pushDir);
        }

        Release();
    }
}
