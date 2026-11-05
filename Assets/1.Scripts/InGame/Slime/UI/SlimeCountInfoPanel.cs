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
      GameEventBus.Subscribe<TileObjectChangedEvent>(e => UpdatePanel()); // 카운팅 제외 타일에 올리거나 내리면 수가 바뀜
      GameEventBus.Subscribe<TileItemChangedEvent>(e => UpdatePanel()); // 슬라임이 있는 칸에 설치/판매
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
        infoText.text = $"{SlimeSpawner.Instance.CountedSlimeCount}/{SlimeSpawner.Instance.slimeMaxCount}";
    }
}
