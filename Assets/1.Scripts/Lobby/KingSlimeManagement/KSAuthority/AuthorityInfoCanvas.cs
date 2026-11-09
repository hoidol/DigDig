using System;
using UnityEngine;
using UnityEngine.Serialization;
using Lobby;

// 권능 정보 + 선택. 같은 묶음의 이전 선택은 바뀜, 잠긴 묶음은 선택 불가 (해금 레벨 안내)
public class AuthorityInfoCanvas : CanvasUI<AuthorityInfoCanvas>
{
    [FormerlySerializedAs("equipmentInfoPanel")] public AuthorityInfoPanel authorityInfoPanel;
    [FormerlySerializedAs("equippedPanel")] public GameObject selectedPanel;
    [FormerlySerializedAs("equipButton")] public GameObject selectButton;

    AuthorityData authorityData;

    public void OpenCanvas(AuthorityData authorityData, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        this.authorityData = authorityData;
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        bool selected = UserDataManager.Instance.userAuthorityManager.IsSelected(authorityData);
        selectedPanel.SetActive(selected);
        selectButton.SetActive(!selected);
        authorityInfoPanel.SetPanel(authorityData);
    }

    // 버튼 onClick (이전 OnClickedEquipButton에 연결된 것도 그대로 동작)
    public void OnClickedEquipButton() => OnClickedSelectButton();

    public void OnClickedSelectButton()
    {
        AuthoritySelectResult result = UserDataManager.Instance.userAuthorityManager.Select(authorityData);
        if (result == AuthoritySelectResult.Locked)
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Authority_Locked"), AuthorityData.GetUnlockLevel(authorityData.tier)));
        LobbyManager.Instance.GetLobbyCanvas(LobbyState.KingSlimeManagement).UpdateCanvas();
        UpdateCanvas();
    }
}
