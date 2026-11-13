using System;
using UnityEngine;

// 인게임 직접 강화 (CLAUDE.md "인게임 직접 강화"): 장착한 슬라임 5종을 패널마다 하나씩 보여주고 코인으로 종(key) 단위 강화
// (2026-11-11) 슬롯머신(릴 3개를 돌려 대상·종류·수치 결정) 기능은 사용 안 함 → 아래 주석
public class ReinforceCanvas : CanvasUI<ReinforceCanvas>
{
    public ReinforcePanel[] reinforcePanels; // 장착 슬롯 순서대로 (비워 두면 하위 ReinforcePanel 전부)

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        if (reinforcePanels == null || reinforcePanels.Length == 0)
            reinforcePanels = GetComponentsInChildren<ReinforcePanel>(true);
        UpdatePanels();
    }

    // 장착 슬라임(UserMinionData.equiptedMinions, 5칸)을 패널에 순서대로. 장착이 비었거나 패널이 남으면 숨김
    public void UpdatePanels()
    {
        UserMinion[] equipped = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
        for (int i = 0; i < reinforcePanels.Length; i++)
        {
            ReinforcePanel panel = reinforcePanels[i];
            if (panel == null)
                continue;
            MinionData data = equipped != null && i < equipped.Length && equipped[i] != null ? equipped[i].MinionData : null;
            panel.gameObject.SetActive(data != null);
            if (data != null)
                panel.SetMinionData(data);
        }
    }

    // ---- 슬롯머신 (사용 안 함) ----
    // public ReinforceSlotContainer[] reinforceSlotContainers;
    // public float containerStartInterval = 1f; // 릴마다 시작 간격 (초)
    // public float closeDelay = 2f; // 결과 표시 후 닫기까지 (초)
    //
    // bool isReinforcing;
    //
    // public override void OpenCanvas(Action closeCallback = null)
    // {
    //     base.OpenCanvas(closeCallback);
    //     reinforceSlotContainers = GetComponentsInChildren<ReinforceSlotContainer>();
    //     foreach (ReinforceSlotContainer container in reinforceSlotContainers)
    //         container.SetSlot();
    //     StartReinforce();
    // }
    //
    // public void StartReinforce()
    // {
    //     if (isReinforcing)
    //         return;
    //     StartReinforceAsync().Forget();
    // }
    //
    // // 릴을 containerStartInterval 간격으로 차례로 돌리고, 전부 멈추면 결과 적용 → closeDelay 후 닫기
    // async UniTaskVoid StartReinforceAsync()
    // {
    //     isReinforcing = true;
    //     UniTask[] spins = reinforceSlotContainers.Select((container, i) => SpinDelayed(container, i * containerStartInterval)).ToArray();
    //     await UniTask.WhenAll(spins);
    //     ApplyReinforce(GetResultKey(ReinforceSlotType.Target), GetResultKey(ReinforceSlotType.Buff), GetResultKey(ReinforceSlotType.Value));
    //     bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(closeDelay), DelayType.UnscaledDeltaTime, cancellationToken: destroyCancellationToken).SuppressCancellationThrow();
    //     isReinforcing = false;
    //     CloseCanvas();
    // }
    //
    // async UniTask SpinDelayed(ReinforceSlotContainer container, float delay)
    // {
    //     var token = destroyCancellationToken;
    //     if (delay > 0 && await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.UnscaledDeltaTime, cancellationToken: token).SuppressCancellationThrow())
    //         return;
    //     await container.Spin(token);
    // }
    //
    // string GetResultKey(ReinforceSlotType type)
    // {
    //     ReinforceSlotContainer container = reinforceSlotContainers.FirstOrDefault(c => c.reinforceSlotType == type);
    //     return container != null ? container.ResultKey : null;
    // }
    //
    // // 슬롯 결과 적용 (대상 / 강화 종류 / 수치)
    // void ApplyReinforce(string target, string buffType, string value)
    // {
    //     if (target == ReinforceSlot.NONE_KEY || buffType == ReinforceSlot.NONE_KEY)
    //         return; // 꽝
    // }
}
