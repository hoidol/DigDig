using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
public class ActiveTimer
{
    CancellationTokenSource cts;

    // public void StartTimer(ActiveItem activeItem)
    // {
    //     Stop();
    //     cts = new CancellationTokenSource();
    //     ActiveTime(activeItem, cts.Token).Forget();
    // }

    // public async UniTask ActiveTime(ActiveItem activeItem, CancellationToken token)
    // {
    //     activeItem.CoolTimer = activeItem.coolTime;
    //     while (true)
    //     {
    //         if (activeItem.CoolTimer <= 0)
    //             break;

    //         await UniTask.Yield(token);
    //         activeItem.CoolTimer -= Time.deltaTime;
    //     }

    //     if (token.IsCancellationRequested) return;
    //     activeItem.OnActive();
    //     await UniTask.Delay(TimeSpan.FromSeconds(activeItem.activeTime), cancellationToken: token);

    //     if (token.IsCancellationRequested) return;
    //     activeItem.OnInactive();
    // }

    // public void Stop()
    // {
    //     cts?.Cancel();
    //     cts = null;
    // }
}