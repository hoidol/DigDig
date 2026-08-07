using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentCompareCanvas : CanvasUI<EquipmentCompareCanvas>
{
    //현재
    [SerializeField] EquipmentInfoPanel curEquipmentInfoPanel; // EquipmentAbilityCompareInfoPanel로 Ability 보여주기 

    //착용중
    [SerializeField] EquipmentInfoPanel equippedEquipmentInfoPanel;

    UserEquipment curUserEquipment;
    public GameObject equipButton;
    
    public void OpenCanvas(UserEquipment curUserEquipment, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        this.curUserEquipment = curUserEquipment;
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        curEquipmentInfoPanel.SetPanel(curUserEquipment);
        UserEquipment equippedUserEquipment = UserManager.Instance.userEquipmentManager.GetEquippedUserEquipment(curUserEquipment.equipmentData.equipmentType);
        equippedEquipmentInfoPanel.gameObject.SetActive(equippedUserEquipment != null); 
        if (equippedUserEquipment != null)
        {   
            equippedEquipmentInfoPanel.SetPanel(equippedUserEquipment);
        }
        
    }
    //현재 보고 있는 장비로 교체하기
    public void OnClickedEquipButton()
    {
        UserManager.Instance.userEquipmentManager.EquiptUserEquipment(curUserEquipment);
        EquipmentCanvas.Instance.UpdateCanvas();
        UpdateCanvas();
        CloseCanvas();
    }
}