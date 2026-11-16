using Cysharp.Threading.Tasks;
using UnityEngine;

// 인게임 직접 강화 (CLAUDE.md "인게임 직접 강화"): InGame 화면에 항상 보이는 강화 칸 묶음
// 장착한 슬라임 5종을 장착 슬롯 순서대로 ReinforcePanel에 하나씩 보여주고, 칸 버튼으로 코인을 내고 종(key) 단위 강화
// (2026-11-14) 이전엔 ReinforceOpenButton → ReinforceCanvas를 열어서 보여줌
public class ReinforceContainer : MonoSingleton<ReinforceContainer>
{
    public ReinforcePanel[] reinforcePanels; // 장착 슬롯 순서대로 (비워 두면 하위 ReinforcePanel 전부)

    async void Start()
    {
        // 미니언 데이터(MinionManager, Addressables 비동기 로드)와 강화 레벨(KingSlime.statMgr, KingSlime.Start에서 생성)이 준비될 때까지 대기
        // MinionManager.LoadTask는 GameManager가 await 중이라 여기서 같이 await하면 UniTask 예외(동시에 두 곳 await 불가) → 데이터가 채워졌는지만 확인
        bool canceled = await UniTask.WaitUntil(() => MinionManager.Instance.minionDataDic.Count > 0
                && KingSlime.Instance != null && KingSlime.Instance.statMgr != null,
            cancellationToken: destroyCancellationToken).SuppressCancellationThrow();
        if (canceled || this == null)
            return;
        UpdatePanels();
    }

    // 장착 슬라임(UserMinionData.equiptedMinions, 5칸)을 패널에 순서대로. 장착이 비었거나 패널이 남으면 숨김
    public void UpdatePanels()
    {
        if (reinforcePanels == null || reinforcePanels.Length == 0)
            reinforcePanels = GetComponentsInChildren<ReinforcePanel>(true);

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
}
