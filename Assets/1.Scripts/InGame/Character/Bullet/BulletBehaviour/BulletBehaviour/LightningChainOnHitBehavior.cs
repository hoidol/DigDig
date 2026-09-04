using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 적중한 대상에서 시작해 주변 타겟으로 번개가 연쇄 적중, LineRenderer로 궤적 표시
public class LightningChainOnHitBehavior 
{
    const float LASER_SPEED = 40f; // 레이저가 다음 지점까지 뻗어나가는 속도
    const float LINE_DURATION = 0.05f; // 다 뻗은 뒤 사라지기 전 유지 시간

    readonly float initSearchRadius;
    readonly float searchRadius;
    readonly int lightningCount;
    readonly float damage;
    readonly LayerMask hitLayerMask;

    public LightningChainOnHitBehavior(float initSearchRadius, float searchRadius, int lightningCount, float damage, LayerMask hitLayerMask)
    {
        this.initSearchRadius = initSearchRadius;
        this.searchRadius = searchRadius;
        this.lightningCount = lightningCount;
        this.damage = damage;
        this.hitLayerMask = hitLayerMask;
    }

    public bool OnHit(BulletObject bullet, Vector2 startPos)
    {
        List<Vector3> points = new List<Vector3> { startPos };
        List<IHittable> hitTargets = new List<IHittable>();

        Vector2 originPos = startPos;
        float radius = initSearchRadius;

        for (int i = 0; i < lightningCount; i++)
        {
            IHittable next = FindNearestTarget(originPos, radius, hitTargets);
            if (next == null)
                break;

            points.Add(next.Transform.position);
            hitTargets.Add(next);

            originPos = next.Transform.position;
            radius = searchRadius;
        }

        LightningBulletObject lightningBullet = bullet as LightningBulletObject;
        if (lightningBullet != null && lightningBullet.lineRenderer != null && points.Count > 1)
        {
            bullet.direction = Vector2.zero;
            lightningBullet.StartCoroutine(ShowLine(lightningBullet, points, hitTargets));
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

    IEnumerator ShowLine(LightningBulletObject bullet, List<Vector3> points, List<IHittable> hitTargets)
    {
        LineRenderer lineRenderer = bullet.lineRenderer;
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, points[0]);

        DamageData chainDamageData = new DamageData();
        chainDamageData.damage = damage;

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
            target.TakeDamage(chainDamageData);
        }

        yield return new WaitForSeconds(LINE_DURATION);

        lineRenderer.enabled = false;
        bullet.Release();
    }
}
