using UnityEngine;
using System;
using System.Collections;

// 투사체 난사 패턴
// 원형 + 랜덤 spread 발사
// 페이즈가 높을수록 발수 증가
public class BulletSprayPattern : BossAttackPattern
{
    [SerializeField] int baseCount = 8;
    [SerializeField] int countPerPhase = 4;         // 페이즈마다 추가
    [SerializeField] float spreadAngle = 25f;        // 랜덤 흔들림 각도
    [SerializeField] float minSpeedMult = 0.8f;
    [SerializeField] float maxSpeedMult = 1.3f;
    [SerializeField] float burstInterval = 0.05f;   // 발사 간격 (너무 한꺼번에 나오지 않도록)

    Coroutine coroutine;

    public override void Execute(Boss boss, Action onEnd)
    {
        int count = baseCount + boss.CurrentPhase * countPerPhase;
        float damage = boss.enemyData.GetAttackPower();
        coroutine = StartCoroutine(DoBurst(boss, count, damage, onEnd));
    }

    IEnumerator DoBurst(Boss boss, int count, float damage, Action onEnd)
    {
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i + UnityEngine.Random.Range(-spreadAngle, spreadAngle);
            Vector2 dir = (Quaternion.Euler(0, 0, angle) * Vector2.right).normalized;

            // 속도 랜덤 - EnemyBullet의 moveSpeed는 내부 값이므로 direction 스케일로 대신 표현
            // (BulletBase.Move가 direction * moveSpeed 이므로 dir 크기는 의미 없음)
            var bullet = EnemyBullet.Instantiate();
            bullet.transform.position = boss.transform.position;
            bullet.Shoot(dir, damage);

            yield return new WaitForSeconds(burstInterval);
        }

        coroutine = null;
        onEnd?.Invoke();
    }

    public override void Cancel()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
