using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class ActiveTimer
{
    public void StartTimer(ActiveItem activeItem)
    {
        ActiveTime(activeItem).Forget();
    }
    public async UniTask ActiveTime(ActiveItem activeItem)
    {
        activeItem.CoolTimer = activeItem.coolTime;
        while (true)
        {
            if (activeItem.CoolTimer <= 0)
                break;
            await UniTask.Yield();
            activeItem.CoolTimer -= Time.deltaTime;
        }
        activeItem.OnActive();
        await UniTask.Delay(TimeSpan.FromSeconds(activeItem.activeTime));
        activeItem.OnInactive();
    }
}