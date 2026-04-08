using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 낙하 공격 패턴
// 플레이어 주변 랜덤 위치에 경고 표시 → 일정 시간 후 충격
// 페이즈가 높을수록 낙하 수 증가
public class RandomDropPattern : BossAttackPattern
{
    [SerializeField] DropWarning warningPrefab;
    [SerializeField] int baseDropCount = 3;
    [SerializeField] int countPerPhase = 2;     // 페이즈마다 추가
    [SerializeField] float spawnRadius = 8f;    // 플레이어 기준 반경
    [SerializeField] float spawnInterval = 0.1f;// 개별 낙하 간격
    [SerializeField] float warningDuration = 0.5f;
    [SerializeField] float hitRadius = 1.5f;
    [SerializeField] float damage = 15f;

    Coroutine coroutine;
    readonly List<DropWarning> activeWarnings = new();

    public override void Execute(Boss boss, Action onEnd)
    {
        int count = baseDropCount + boss.CurrentPhase * countPerPhase;
        float baseDamage = boss.enemyData.GetAttackPower() * 0.8f;
        coroutine = StartCoroutine(DoDrops(count, baseDamage, onEnd));
    }

    IEnumerator DoDrops(int count, float baseDamage, Action onEnd)
    {
        activeWarnings.Clear();
        int remaining = count;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomPos();
            DropWarning warning = Instantiate(warningPrefab, pos, Quaternion.identity);
            activeWarnings.Add(warning);
            warning.Play(warningDuration, hitRadius, baseDamage, () => remaining--);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 모든 낙하 완료 대기
        yield return new WaitUntil(() => remaining <= 0);
        activeWarnings.Clear();
        coroutine = null;
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
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        foreach (var w in activeWarnings)
            if (w != null) w.Cancel();
        activeWarnings.Clear();
    }
}
