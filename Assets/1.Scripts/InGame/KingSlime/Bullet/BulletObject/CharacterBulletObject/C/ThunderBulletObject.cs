using UnityEngine;
using System.Collections;
using System.Linq;

// 천둥 슬라임 낙뢰: 날아가지 않고 발사 방향의 첫 적 위치에 떨어져 주변 범위 피해
// 발사 직후(OnFired에서 타일 효과가 붙은 뒤) STRIKE_DELAY 후에 피해를 준다
public class ThunderBulletObject : AllyBulletObject
{
    ThunderBulletSpec thunderBulletSpec;

    const float STRIKE_DELAY = 0.1f;  // 발사 → 낙뢰까지 (타일 효과 적용 대기 겸 연출)
    const float SHOW_DURATION = 0.15f; // 낙뢰 표시 유지 시간

    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet, allyUnit);
        thunderBulletSpec = bullet as ThunderBulletSpec;
    }

    public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage); // 발사 정보 저장 (연발 타일이 같은 탄을 다시 쏠 때 사용)
        Transform target = FindStrikeTarget(shootPos, dir);
        Vector2 fallbackPos = shootPos + dir * thunderBulletSpec.maxDistance;
        StartCoroutine(Strike(target, fallbackPos));
    }

    // 이동/수명 처리 없음 (Strike 코루틴에서 Release)
    public override void Update()
    {
    }

    // 발사 방향으로 사거리 안 첫 적
    Transform FindStrikeTarget(Vector2 origin, Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, thunderBulletSpec.maxDistance, thunderBulletSpec.hitLayerMask);
        return hit ? hit.collider.transform : null;
    }

    IEnumerator Strike(Transform target, Vector2 fallbackPos)
    {
        yield return new WaitForSeconds(STRIKE_DELAY);

        // 대기 중 타겟이 움직였으면 현재 위치에 떨어짐 (죽었으면 처음 노린 지점)
        Vector2 strikePos = target != null && target.gameObject.activeInHierarchy ? (Vector2)target.position : fallbackPos;
        transform.position = strikePos;
        EffectManager.Instance.Play(EffectType.SmallExplosion, strikePos);
        EmberBrazierAuthority.OnZoneDamage(strikePos, thunderBulletSpec.strikeRadius, damage * damageMultiplier, thunderBulletSpec.hitLayerMask);

        IHittable[] targets = Physics2D.OverlapCircleAll(strikePos, thunderBulletSpec.strikeRadius, thunderBulletSpec.hitLayerMask)
            .Select(c => c.GetComponent<IHittable>())
            .Where(h => h != null)
            .OrderBy(h => Vector2.Distance(strikePos, h.Transform.position))
            .Take(thunderBulletSpec.maxHitCount)
            .ToArray();

        foreach (IHittable hit in targets)
        {
            EffectManager.Instance.Play(EffectType.Spark, hit.Transform.position);

            float finalDamage = damage * damageMultiplier;
            for (int j = 0; j < forces.Count; j++)
                finalDamage += forces[j].GetMultiDamage(this, hit, default, direction);
            if (finalDamage < 1f)
                finalDamage = 1f;

            // Init은 하지 않음: OnFired에서 넣은 타일 효과(치명타, 만능, 거인 사냥 등)가 지워짐
            allyUnit.AccumulateDamage(finalDamage);
            allyUnitDamageData.damage = finalDamage;
            hit.TakeDamage(allyUnitDamageData);

            // 적중 후 효과 (타일의 둔화/스턴/처치 코인 등). 범위 피해라 Release 여부는 무시
            foreach (var b in behaviors)
                b.OnHit(this, hit, default, direction);
        }

        yield return new WaitForSeconds(SHOW_DURATION);
        Release();
    }
}
