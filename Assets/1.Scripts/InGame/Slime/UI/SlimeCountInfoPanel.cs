using TMPro;
using UnityEngine;

public class SlimeCountInfoPanel : MonoBehaviour
{
    public TMP_Text infoText;
    void Start()
    {
      GameEventBus.Subscribe<SpawnSlimeEvent>(OnSpawnSlimeEvent);  
      GameEventBus.Subscribe<RemoveSlimeEvent>(OnRemoveSlimeEvent);
      GameEventBus.Subscribe<StartGameEvent>(e => UpdatePanel()); // 로비 업그레이드(MaxSlimeCount)로 상한이 바뀜
      GameEventBus.Subscribe<AddedItemEvent>(e => UpdatePanel()); // 슬라임 수 증가 아이템으로 상한이 바뀜
      UpdatePanel();
    }

    void OnSpawnSlimeEvent(SpawnSlimeEvent e)
    {
        UpdatePanel();
    }
    void OnRemoveSlimeEvent(RemoveSlimeEvent e)
    {
        UpdatePanel();
    }

    void UpdatePanel()
    {
        infoText.text = $"{SlimeFieldController.Instance.CountedSlimeCount}/{SlimeFieldController.Instance.slimeMaxCount}";
    }
}
