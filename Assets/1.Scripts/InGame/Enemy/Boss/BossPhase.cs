using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;


public class BossPhase : MonoBehaviour
{
    public float phaseThreshold;
    public AttackPatternInfo[]  attackPatternInfos;
    Boss boss;
    CancellationTokenSource cts;

    public void Init(Boss boss)
    {
        this.boss = boss;
    }
    public void StartPhase()
    {
        cts = new CancellationTokenSource();
        ProcessPhase(cts.Token).Forget();
    }

    async UniTaskVoid ProcessPhase(CancellationToken ct)
    {
        while (true)
        {
            int index = Random.Range(0, attackPatternInfos.Length);

            boss.animator.Play(attackPatternInfos[index].readyAnimName);
            await UniTask.WaitForSeconds(attackPatternInfos[index].readyTime, cancellationToken: ct);

            attackPatternInfos[index].attackPattern.Execute(boss, () =>
            {

            });
            await UniTask.WaitForSeconds(attackPatternInfos[index].attackPattern.duration, cancellationToken: ct);
        }
    }

    public void EndPhase()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}

