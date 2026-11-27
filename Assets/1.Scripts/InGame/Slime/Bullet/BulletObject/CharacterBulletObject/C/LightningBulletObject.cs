using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class LightningBulletObject : AllyBulletObject
{
    LightningBulletSpec lightningBulletSpec;
    public LineRenderer lineRenderer;
    [Tooltip("전기 이펙트(LightningLine_Yellow 프리팹). 있으면 lineRenderer 대신 사용")]
    public LightningLine lightningLine;

    const float LASER_SPEED = 40f; // 레이저가 다음 지점까지 뻗어나가는 속도
    const float LINE_DURATION = 0.05f; // 다 뻗은 뒤 사라지기 전 유지 시간
    const float LIGHTNING_LINE_DURATION = 0.15f; // 전기 이펙트는 지지직이 보이도록 조금 더 유지

    readonly List<Vector3> linePoints = new List<Vector3>();

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
        if (lineRenderer != null)
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
            // 첫 타겟 = 발사 방향의 적 (다중 타겟 타일이면 탄마다 다른 타겟을 향해 쏘므로 연쇄도 그 적부터 따로 시작)
            // 이전엔 항상 가장 가까운 적부터라 탄을 여러 발 쏘면 똑같은 연쇄가 겹쳐 나왔음
            IHittable next = i == 0 ? FindFirstTarget(originPos, radius, direction) : FindNearestTarget(originPos, radius, hitTargets);
            if (next == null)
                break;

            points.Add(Enemy.GetHitPosition(next)); // 선은 몸통(HitPoint)으로
            hitTargets.Add(next);

            originPos = next.Transform.position;
            radius = lightningBulletSpec.searchRadius;
        }

        if ((lightningLine != null || lineRenderer != null) && points.Count > 1)
        {
            direction = Vector2.zero;
            StartCoroutine(ShowLine(points, hitTargets));
            return false;
        }

        return true;
    }

    // 발사 방향과 각도가 가장 가까운 적 (같으면 가까운 적). 방향이 없으면 가장 가까운 적
    IHittable FindFirstTarget(Vector2 pos, float radius, Vector2 dir)
    {
        if (dir == Vector2.zero)
            return FindNearestTarget(pos, radius, new List<IHittable>());
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, radius, hitLayerMask);
        return cols
            .Select(c => c.GetComponent<IHittable>())
            .Where(h => h != null)
            .OrderBy(h => Vector2.Angle(dir, (Vector2)h.Transform.position - pos))
            .ThenBy(h => Vector2.Distance(pos, h.Transform.position))
            .FirstOrDefault();
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
        bool useLightningLine = lightningLine != null;
        if (useLightningLine)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
            linePoints.Clear();
            linePoints.Add(points[0]);
            linePoints.Add(points[0]);
            lightningLine.SetPoints(linePoints);
            lightningLine.Play();
        }
        else
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, points[0]);
        }

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 from = points[i - 1];
            Vector3 to = points[i];
            float duration = Mathf.Max(Vector3.Distance(from, to) / LASER_SPEED, 0.01f);

            if (useLightningLine)
            {
                // 마지막 점 = 뻗어 나가는 끝(EndFx가 따라감)
                if (i > 1)
                    linePoints.Add(from);
            }
            else
                lineRenderer.positionCount = i + 1;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetLineTip(useLightningLine, i, Vector3.Lerp(from, to, t / duration));
                yield return null;
            }
            SetLineTip(useLightningLine, i, to);

            // 레이저가 도달한 순간 데미지 적용
            IHittable target = hitTargets[i - 1];
            EffectManager.Instance.Play(EffectType.Thunder, to);
            
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

        yield return new WaitForSeconds(useLightningLine ? LIGHTNING_LINE_DURATION : LINE_DURATION);

        if (useLightningLine)
            lightningLine.Stop();
        else
            lineRenderer.enabled = false;
        Release();
    }

    void SetLineTip(bool useLightningLine, int idx, Vector3 pos)
    {
        if (useLightningLine)
        {
            linePoints[linePoints.Count - 1] = pos;
            lightningLine.SetPoints(linePoints);
        }
        else
            lineRenderer.SetPosition(idx, pos);
    }

}

