using System.Collections.Generic;
using UnityEngine;

// 게임 월드 위에 뜨는 UI 캔버스
// 미니언 머리 위 상태(MinionGameStatePanel): 하위에 둔 템플릿 하나를 미니언마다 복사(풀링)
// Minion.Spawn → AttachMinionState, 미니언 비활성화(합성·풀 반납) → ReleaseMinionState
public class GameWorldCanvas : MonoSingleton<GameWorldCanvas>
{
    public MinionGameStatePanel minionStateTemplate; // 비워 두면 하위의 첫 MinionGameStatePanel

    readonly Stack<MinionGameStatePanel> minionStatePool = new();
    readonly Dictionary<Minion, MinionGameStatePanel> activeMinionStates = new();

    void Awake()
    {
        if (minionStateTemplate == null)
            minionStateTemplate = GetComponentInChildren<MinionGameStatePanel>(true);
        if (minionStateTemplate != null && minionStateTemplate.gameObject.scene.IsValid()) // 씬에 둔 템플릿만 숨김 (프리팹 에셋은 건드리지 않음)
            minionStateTemplate.gameObject.SetActive(false);
        else if (minionStateTemplate == null)
            Debug.LogWarning("GameWorldCanvas 하위에 MinionGameStatePanel 템플릿 없음 → 미니언 머리 위 상태 표시 안 함");
    }

    public void AttachMinionState(Minion minion)
    {
        if (minion == null || minionStateTemplate == null)
            return;
        if (!activeMinionStates.TryGetValue(minion, out MinionGameStatePanel panel) || panel == null)
        {
            panel = minionStatePool.Count > 0 ? minionStatePool.Pop() : Instantiate(minionStateTemplate, transform, false);
            activeMinionStates[minion] = panel;
        }
        panel.Bind(minion);
    }

    public void ReleaseMinionState(Minion minion)
    {
        if (ReferenceEquals(minion, null) || !activeMinionStates.Remove(minion, out MinionGameStatePanel panel))
            return;
        if (panel == null)
            return;
        panel.Unbind();
        minionStatePool.Push(panel);
    }

    public bool HasMinionState(Minion minion) => minion != null && activeMinionStates.ContainsKey(minion);
}
