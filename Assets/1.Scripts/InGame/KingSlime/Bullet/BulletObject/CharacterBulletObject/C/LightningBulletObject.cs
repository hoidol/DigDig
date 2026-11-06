using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class LightningBulletObject : AllyBulletObject
{
    LightningBulletSpec lightningBulletSpec;
    public LineRenderer lineRenderer;

    const float LASER_SPEED = 40f; // 레이저가 다음 지점까지 뻗어나가는 속도
    const float LINE_DURATION = 0.05f; // 다 뻗은 뒤 사라지기 전 유지 시간

    // LightningChainOnHitBehavior lightningChainOnHitBehavior;
    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        if (bullet == null)
        {
            Debug.Log("LightningBulletObject SetBullet() if(bullet == null)");
        }
        base.SetBullet(bullet, allyUnit);
        lightningBulletSpec = bullet as LightningBulletSpec;
    }

    public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage); // 발사 정보 저장 (연발 타일이 같은 탄을 다시 쏠 때 사용)
        allyUnitDamageData.damage = damage;
        lineRenderer.positionCount = 0;
        OnHit(allyUnit.Transform.position);
        // lightningChainOnHitBehavior.OnHit(this, transform.position);
    }

    public override void Update()
    {

    }

    public bool OnHit(Vector2 startPos)
    {
        List<Vector3> points = new List<Vector3> { startPos };
        List<IHittable> hitTargets = new List<IHittable>();

        Vector2 originPos = startPos;
        float radius = lightningBulletSpec.initSearchRadius;

        for (int i = 0; i < lightningBulletSpec.lightningCount; i++)
        {
            IHittable next = FindNearestTarget(originPos, radius, hitTargets);
            if (next == null)
                break;

            points.Add(next.Transform.position);
            hitTargets.Add(next);

            originPos = next.Transform.position;
            radius = lightningBulletSpec.searchRadius;
        }

        if (lineRenderer != null && points.Count > 1)
        {
            direction = Vector2.zero;
            StartCoroutine(ShowLine(points, hitTargets));
            return false;
        }

        return true;
    }

    IHittable FindNearestTarget(Vector2 pos, float radius, List<IHittable> exclude)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, radius, hitLayerMask);
        return cols
            .Select(c => c.GetComponent<IHittable>())
            .Where(h => h != null && !exclude.Contains(h))
            .OrderBy(h => Vector2.Distance(pos, h.Transform.position))
            .FirstOrDefault();
    }

    IEnumerator ShowLine(List<Vector3> points, List<IHittable> hitTargets)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, points[0]);

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 from = points[i - 1];
            Vector3 to = points[i];
            float duration = Mathf.Max(Vector3.Distance(from, to) / LASER_SPEED, 0.01f);

            lineRenderer.positionCount = i + 1;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                lineRenderer.SetPosition(i, Vector3.Lerp(from, to, t / duration));
                yield return null;
            }
            lineRenderer.SetPosition(i, to);

            // 레이저가 도달한 순간 데미지 적용
            IHittable target = hitTargets[i - 1];
            EffectManager.Instance.Play(EffectType.Spark, target.Transform.position);
            
            float finalDamage = damage * damageMultiplier;
            for (int j = 0; j < forces.Count; j++)
                finalDamage += forces[j].GetMultiDamage(this, target, default, direction);
            if (finalDamage < 1f)
                finalDamage = 1f;

            // Init은 하지 않음: OnFired에서 넣은 타일 효과(치명타, 만능 등)가 지워짐
            allyUnit.AccumulateDamage(finalDamage);
            allyUnitDamageData.damage = finalDamage;
            target.TakeDamage(allyUnitDamageData); // 데미지 표시(ApplyDamage)는 TakeDamage 안에서 처리

            // 적중 후 효과 (타일의 둔화, 스턴, 처치 코인 등). 연쇄 탄이라 Release 여부는 무시
            foreach (var b in behaviors)
                b.OnHit(this, target, default, direction);
        }

        yield return new WaitForSeconds(LINE_DURATION);

        lineRenderer.enabled = false;
        Release();
    }

}

