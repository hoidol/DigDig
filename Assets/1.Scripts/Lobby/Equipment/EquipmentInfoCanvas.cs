using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentInfoCanvas : CanvasUI<EquipmentInfoCanvas>
{
    public EquipmentInfoPanel equipmentInfoPanel;
    public GameObject equippedPanel;

    UserEquipment userEquipment;
    public GameObject equipButton;
    public GameObject releaseButton;
    public void OpenCanvas(UserEquipment userEquipment, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        this.userEquipment = userEquipment;
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        equippedPanel.SetActive(userEquipment.equipped);
        if (userEquipment.equipped)
        {
            equipButton.SetActive(false);
            releaseButton.SetActive(true);
        }
        else
        {
            equipButton.SetActive(true);
            releaseButton.SetActive(false);
        }
        equipmentInfoPanel.SetPanel(userEquipment);
    }

    public void OnClickedEquipButton()
    {
        UserManager.Instance.userEquipmentManager.EquiptUserEquipment(userEquipment);
        LobbyManager.Instance.GetLobbyCanvas(LobbyState.Equipment).UpdateCanvas();
        UpdateCanvas();
    }
    public void OnClickedReleaseButton()
    {
        UserManager.Instance.userEquipmentManager.ReleaseUserEquipment(userEquipment.id);
        
        LobbyManager.Instance.GetLobbyCanvas(LobbyState.Equipment).UpdateCanvas();
        UpdateCanvas();
    }
}