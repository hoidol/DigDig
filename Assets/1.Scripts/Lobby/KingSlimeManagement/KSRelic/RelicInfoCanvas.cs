using System;
using UnityEngine;
using UnityEngine.Serialization;
using Lobby;

// 유물 정보 + 선택. 같은 묶음의 이전 선택은 바뀜, 잠긴 묶음은 선택 불가 (해금 레벨 안내)
public class RelicInfoCanvas : CanvasUI<RelicInfoCanvas>
{
    [FormerlySerializedAs("equipmentInfoPanel")] public RelicInfoPanel relicInfoPanel;
    [FormerlySerializedAs("equippedPanel")] public GameObject selectedPanel;
    [FormerlySerializedAs("equipButton")] public GameObject selectButton;

    RelicData relicData;

    public void OpenCanvas(RelicData relicData, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        this.relicData = relicData;
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        bool selected = UserDataManager.Instance.userRelicManager.IsSelected(relicData);
        selectedPanel.SetActive(selected);
        selectButton.SetActive(!selected);
        relicInfoPanel.SetPanel(relicData);
    }

    // 버튼 onClick (이전 OnClickedEquipButton에 연결된 것도 그대로 동작)
    public void OnClickedEquipButton() => OnClickedSelectButton();

    public void OnClickedSelectButton()
    {
        RelicSelectResult result = UserDataManager.Instance.userRelicManager.Select(relicData);
        if (result == RelicSelectResult.Locked)
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Relic_Locked"), RelicData.GetUnlockLevel(relicData.tier)));
        LobbyManager.Instance.GetLobbyCanvas(LobbyState.KingSlimeManagement).UpdateCanvas();
        UpdateCanvas();
    }
}
