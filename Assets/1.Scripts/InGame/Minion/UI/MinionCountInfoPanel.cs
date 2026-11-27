using TMPro;
using UnityEngine;

public class MinionCountInfoPanel : MonoBehaviour
{
    public TMP_Text infoText;
    void Start()
    {
        GameEventBus.Subscribe<SpawnMinionEvent>(OnSpawnMinionEvent);
        GameEventBus.Subscribe<RemoveMinionEvent>(OnRemoveMinionEvent);
        GameEventBus.Subscribe<StartGameEvent>(OnStartGame); // 시작 시 상한 확정
        GameEventBus.Subscribe<AddedRuneEvent>(OnAddedRune); // 슬라임 수 증가 아이템으로 상한이 바뀜
        GameEventBus.Subscribe<MinionMaxCountChangedEvent>(OnMaxCountChanged); // (2026-11-25) 용사 2단계 해방 등 상한 변경
        GameEventBus.Subscribe<LevelUpCanvasClosedEvent>(OnLevelUpClosed); // (2026-11-25) 레벨업 창을 닫으면 갱신
        UpdatePanel();
    }
    void OnDestroy()
    {
        GameEventBus.Unsubscribe<SpawnMinionEvent>(OnSpawnMinionEvent);
        GameEventBus.Unsubscribe<RemoveMinionEvent>(OnRemoveMinionEvent);
        GameEventBus.Unsubscribe<StartGameEvent>(OnStartGame);
        GameEventBus.Unsubscribe<AddedRuneEvent>(OnAddedRune);
        GameEventBus.Unsubscribe<MinionMaxCountChangedEvent>(OnMaxCountChanged);
        GameEventBus.Unsubscribe<LevelUpCanvasClosedEvent>(OnLevelUpClosed);
    }
    // 꺼져 있던 동안 바뀐 값도 다시 켜질 때 반영
    void OnEnable()
    {
        if (MinionFieldController.Instance != null && Slime.Instance != null)
            UpdatePanel();
    }

    void OnSpawnMinionEvent(SpawnMinionEvent e) => UpdatePanel();
    void OnRemoveMinionEvent(RemoveMinionEvent e) => UpdatePanel();
    void OnStartGame(StartGameEvent e) => UpdatePanel();
    void OnAddedRune(AddedRuneEvent e) => UpdatePanel();
    void OnMaxCountChanged(MinionMaxCountChangedEvent e) => UpdatePanel();
    void OnLevelUpClosed(LevelUpCanvasClosedEvent e) => UpdatePanel();

    void UpdatePanel()
    {
        if (infoText == null || MinionFieldController.Instance == null || Slime.Instance == null || Slime.Instance.minionInventory == null)
            return;
        infoText.text = $"{MinionFieldController.Instance.CountedMinionCount}/{MinionFieldController.Instance.minionMaxCount}";
    }
}
