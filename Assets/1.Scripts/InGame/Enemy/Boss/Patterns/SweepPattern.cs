using UnityEngine;
using System;

// 크게 휘두르기 패턴
// 플레이어 방향으로 부채꼴 범위 공격
// 페이즈가 높을수록 범위와 피해 증가
public class SweepPattern : BossAttackPattern
{
    [SerializeField] AttackIndicator sweepIndicator;
    [SerializeField] float baseRadius = 3.5f;
    [SerializeField] float radiusPerPhase = 0.5f;
    [SerializeField] float sweepAngle = 120f;           // 부채꼴 각도
    [SerializeField] float damageMultiplier = 1.5f;

    public override void Execute(Boss boss, Action onEnd)
    {
        float radius = baseRadius + boss.CurrentPhase * radiusPerPhase;
        float damage = boss.enemyData.GetAttackPower() * damageMultiplier;
        Vector2 aimDir = (Player.Instance.transform.position - boss.transform.position).normalized;

        sweepIndicator.PlayIndicator(() =>
        {
            DealDamage(boss.transform.position, aimDir, radius, damage);
            onEnd?.Invoke();
        });
    }

    void DealDamage(Vector2 origin, Vector2 aimDir, float radius, float damage)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(origin, radius);
        foreach (var col in cols)
        {
            if (!col.CompareTag("Player")) continue;

            Vector2 toTarget = ((Vector2)col.transform.position - origin).normalized;
            if (Vector2.Angle(aimDir, toTarget) <= sweepAngle * 0.5f)
                Player.Instance.TakeDamage(damage);
        }
    }

    public override void Cancel()
    {
        sweepIndicator.StopIndicator();
    }
}
