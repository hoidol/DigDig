using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ReinforceCanvas : CanvasUI<ReinforceCanvas>
{
    public ReinforceSlotContainer[] reinforceSlotContainers;
    public float containerStartInterval = 1f; // 릴마다 시작 간격 (초)
    public float closeDelay = 2f; // 결과 표시 후 닫기까지 (초)

    bool isReinforcing;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        reinforceSlotContainers = null;
        reinforceSlotContainers = GetComponentsInChildren<ReinforceSlotContainer>();
        foreach (ReinforceSlotContainer container in reinforceSlotContainers)
            container.SetSlot();

            StartReinforce();
    }

    public void StartReinforce()
    {
        if (isReinforcing)
            return;
        StartReinforceAsync().Forget();
    }

    // 릴을 containerStartInterval 간격으로 차례로 돌리고, 전부 멈추면 결과 적용 → closeDelay 후 닫기
    async UniTaskVoid StartReinforceAsync()
    {
        isReinforcing = true;

        UniTask[] spins = reinforceSlotContainers.Select((container, i) => SpinDelayed(container, i * containerStartInterval)).ToArray();
        await UniTask.WhenAll(spins);

        ApplyReinforce(GetResultKey(ReinforceSlotType.Target), GetResultKey(ReinforceSlotType.Buff), GetResultKey(ReinforceSlotType.Value));

        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(closeDelay), DelayType.UnscaledDeltaTime, cancellationToken: destroyCancellationToken).SuppressCancellationThrow();
        isReinforcing = false;
        CloseCanvas();            
    }

    async UniTask SpinDelayed(ReinforceSlotContainer container, float delay)
    {
        var token = destroyCancellationToken;
        if (delay > 0 && await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.UnscaledDeltaTime, cancellationToken: token).SuppressCancellationThrow())
            return;
        await container.Spin(token);
    }

    string GetResultKey(ReinforceSlotType type)
    {
        ReinforceSlotContainer container = reinforceSlotContainers.FirstOrDefault(c => c.reinforceSlotType == type);
        return container != null ? container.ResultKey : null;
    }

    // 슬롯 결과 적용 (대상 / 강화 종류 / 수치). 실제 적용 방식은 기획 확정 후
    void ApplyReinforce(string target, string buffType, string value)
    {
        Debug.Log($"ReinforceCanvas ApplyReinforce target {target} buffType {buffType} value {value}");
        if (target == ReinforceSlot.NONE_KEY || buffType == ReinforceSlot.NONE_KEY)
            return; // 꽝
        if(target == Character.Instance.characterName.ToString())
        {
            
        }
        // TODO: 적용
    }
}
