using TMPro;
using UnityEngine;

public class SlimeCountInfoPanel : MonoBehaviour
{
    public TMP_Text infoText;
    void Start()
    {
      GameEventBus.Subscribe<SpawnSlimeEvent>(OnSpawnSlimeEvent);  
      GameEventBus.Subscribe<RemoveSlimeEvent>(OnRemoveSlimeEvent);  
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
        infoText.text = $"{Character.Instance.slimeInventory.curSlimes.Count}/{SlimeSpawner.Instance.slimeMaxCount}";
    }
}
