using UnityEngine;
using System;
using System.Collections;
using Cysharp.Threading.Tasks;

// 투사체 난사 패턴
// 원형 + 랜덤 spread 발사
// 페이즈가 높을수록 발수 증가
public class BulletSprayPattern : EnemyAttackPattern
{
    [SerializeField] int count = 8;
    [SerializeField] float spreadAngle = 25f;        // 랜덤 흔들림 각도
    [SerializeField] float minSpeedMult = 0.8f;
    [SerializeField] float maxSpeedMult = 1.3f;
    [SerializeField] float burstInterval = 0.05f;   // 발사 간격 (너무 한꺼번에 나오지 않도록)

    Coroutine coroutine;

    public async override UniTask Execute(IEnemySpecialAttackPattern enemy, Action onEnd)
    {
        await base.Execute(enemy, onEnd);
        float damage = enemy.Transform.GetComponent<Enemy>().enemyData.GetAttackPower();

        // enemy.PlayAnim(readyAnimName);
        coroutine = StartCoroutine(DoBurst(enemy, count, damage, onEnd));

        await UniTask.Delay(TimeSpan.FromSeconds(duration));
    }

    IEnumerator DoBurst(IEnemySpecialAttackPattern enemy, int count, float damage, Action onEnd)
    {
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            enemy.PlayAnim(readyAnimName);
            float angle = angleStep * i + UnityEngine.Random.Range(-spreadAngle, spreadAngle);
            Vector2 dir = (Quaternion.Euler(0, 0, angle) * Vector2.right).normalized;

            // 속도 랜덤 - EnemyBullet의 moveSpeed는 내부 값이므로 direction 스케일로 대신 표현
            // (BulletBase.Move가 direction * moveSpeed 이므로 dir 크기는 의미 없음)
            var bullet = EnemyBullet.Instantiate();
            bullet.transform.position = enemy.Transform.position;
            bullet.Shoot(dir);
            bullet.damage= damage;

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
