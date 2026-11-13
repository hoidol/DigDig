using TMPro;
using UnityEngine;

public class MinionCountInfoPanel : MonoBehaviour
{
    public TMP_Text infoText;
    void Start()
    {
      GameEventBus.Subscribe<SpawnMinionEvent>(OnSpawnMinionEvent);  
      GameEventBus.Subscribe<RemoveMinionEvent>(OnRemoveMinionEvent);
      GameEventBus.Subscribe<StartGameEvent>(e => UpdatePanel()); // 로비 업그레이드(MaxMinionCount)로 상한이 바뀜
      GameEventBus.Subscribe<AddedItemEvent>(e => UpdatePanel()); // 슬라임 수 증가 아이템으로 상한이 바뀜
      UpdatePanel();
    }

    void OnSpawnMinionEvent(SpawnMinionEvent e)
    {
        UpdatePanel();
    }
    void OnRemoveMinionEvent(RemoveMinionEvent e)
    {
        UpdatePanel();
    }

    void UpdatePanel()
    {
        infoText.text = $"{MinionFieldController.Instance.CountedMinionCount}/{MinionFieldController.Instance.minionMaxCount}";
    }
}
