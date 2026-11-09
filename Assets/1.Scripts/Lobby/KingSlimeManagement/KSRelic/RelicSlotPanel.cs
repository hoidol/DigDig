using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

// 유물 목록 한 칸 (한 줄 = 한 묶음). 선택됨 / 잠김(Lv.N) 표시, 누르면 유물 정보 창
public class RelicSlotPanel : MonoBehaviour
{
    [FormerlySerializedAs("equipmentThumPanel")] public RelicThumPanel thumPanel;
    [FormerlySerializedAs("equiptingPanel")][FormerlySerializedAs("equippedPanel")] public GameObject selectedPanel;
    public GameObject lockPanel;
    public TMP_Text lockText;
    public RelicData relicData;

    public void SetRelic(RelicData relicData)
    {
        this.relicData = relicData;
        gameObject.SetActive(relicData != null);
        if (relicData == null)
            return;

        var userRelic = UserDataManager.Instance.userRelicManager;
        bool unlocked = userRelic.IsTierUnlocked(relicData.tier);
        thumPanel.SetRelicData(relicData);
        selectedPanel.SetActive(userRelic.IsSelected(relicData));
        if (lockPanel != null)
            lockPanel.SetActive(!unlocked);
        if (lockText != null)
            lockText.text = $"Lv.{RelicData.GetUnlockLevel(relicData.tier)}";
    }

    public void OnClickedSlot()
    {
        if (relicData != null)
            RelicInfoCanvas.Instance.OpenCanvas(relicData);
    }
}
