using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;
using System.Collections.Generic;


public class BossPhase : MonoBehaviour
{
    public float phaseThreshold;

    public EnemyAttackPattern[] attackPatterns;
    public Boss boss;
    CancellationTokenSource cts;
    // EnemyAttackPattern currentPattern;

    [Header("공격 패턴 쿨타임 범위")]
    [SerializeField] Vector2 attackPatternCooltimeRange;

    public void Init(Boss boss)
    {
        attackPatterns = GetComponentsInChildren<EnemyAttackPattern>();
        this.boss = boss;
    }

    public EnemyAttackPattern GetEnemyAttackPattern(string key)
    {
        for (int i = 0; i < attackPatterns.Length; i++)
        {
            if (attackPatterns[i].key == key)
                return attackPatterns[i];
        }
        return null;
    }

    public void StartPhase()
    {
        // cts = new CancellationTokenSource();
        // ProcessPhase(cts.Token).Forget();
    }
    // List<EnemyAttackPattern> canPatterns = new List<EnemyAttackPattern>();
    // async UniTaskVoid ProcessPhase(CancellationToken ct) // 너무 단조로워
    // {
    //     try
    //     {
    //         while (!ct.IsCancellationRequested)
    //         {
    //             canPatterns.Clear();
    //             foreach (var pattern in attackPatterns)
    //             {
    //                 if (pattern.condition == null || pattern.condition.CheckCondition(boss))
    //                     canPatterns.Add(pattern);
    //             }
    //             int index = Random.Range(0, canPatterns.Count);
    //             currentPattern = canPatterns[index];

    //             await currentPattern.Execute(boss, () =>
    //             {

    //             });
    //             ct.ThrowIfCancellationRequested();
    //             await UniTask.WaitForSeconds(Random.Range(attackPatternCooltimeRange.x, attackPatternCooltimeRange.y));
    //         }
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         // 페이즈 종료로 인한 정상 취소
    //     }
    // }

    public void EndPhase()
    {
        // if (currentPattern != null)
        //     currentPattern.Cancel();
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}

