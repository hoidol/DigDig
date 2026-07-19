using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

// 낙하 공격 패턴
// 플레이어 주변 랜덤 위치에 경고 표시 → 일정 시간 후 충격
// 페이즈가 높을수록 낙하 수 증가
public class RandomDropPattern : EnemyAttackPattern
{
    [SerializeField] int dropCount = 2;
    [SerializeField] float dropSize = 3.5f;
    [SerializeField] float spawnRadius = 3f;    // 플레이어 기준 반경
    [SerializeField] float spawnInterval = 0.5f;// 개별 낙하 간격
    [SerializeField] float warningDuration = 2.5f;

    readonly List<WarningIndicator> activeWarnings = new();

    public async override UniTask Execute(IEnemySpecialAttackPattern enemy, Action onEnd)
    {
        await base.Execute(enemy, onEnd);
        float baseDamage = enemy.Transform.GetComponent<Enemy>().enemyData.GetAttackPower() * 0.8f;

        activeWarnings.Clear();

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 pos = GetRandomPos();
            WarningIndicator warning = WarningIndicator.Instantiate(pos, dropSize);
            activeWarnings.Add(warning);
            warning.Play(warningDuration, (indicator) =>
            {
                enemy.PlayAnim(readyAnimName);
                Collider2D[] cols = Physics2D.OverlapBoxAll(indicator.transform.position, new Vector2(dropSize, dropSize), 0);
                foreach (var col in cols)
                {
                    if (col.CompareTag("Player"))
                    {
                        Player.Instance.TakeDamage(new DamageData() { damage = baseDamage });
                        break;
                    }
                }
                indicator.Cancel();
            });
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval));
        }

        // 모든 낙하 완료 대기
        await UniTask.Delay(TimeSpan.FromSeconds(duration));

        onEnd?.Invoke();
    }

    Vector2 GetRandomPos()
    {
        Vector2 playerPos = Player.Instance.transform.position;
        Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(1f, spawnRadius);
        return playerPos + offset;
    }

    public override void Cancel()
    {
        foreach (var w in activeWarnings)
            if (w != null) w.Cancel();
        activeWarnings.Clear();
    }
}
