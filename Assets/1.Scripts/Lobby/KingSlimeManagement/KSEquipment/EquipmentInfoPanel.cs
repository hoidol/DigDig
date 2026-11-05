using System;
using UnityEngine;
using UnityEngine.UI;

//아이템 이미지 + 능력치 보여주는 창
public class EquipmentInfoPanel : MonoBehaviour
{
    public EquipmentThumPanel equipmentThumPanel;
    UserEquipment userEquipment;
    public EquipmentAbilityInfoPanel[] equipmentAbilityInfoPanels;
    public void SetPanel(UserEquipment userEquipment)
    {
        this.userEquipment = userEquipment;
        UpdatePanel();
    }
    public void UpdatePanel()
    {        
        equipmentThumPanel.SetEquipmentData(userEquipment.equipmentData);
        for(int i =0;i<equipmentAbilityInfoPanels.Length;i++)
        {
            if(i<userEquipment.equipmentData.abilities.Length)
            {
                equipmentAbilityInfoPanels[i].gameObject.SetActive(true);
                equipmentAbilityInfoPanels[i].UpdatePanel(userEquipment.equipmentData);
            }
            else
            {
                equipmentAbilityInfoPanels[i].gameObject.SetActive(false);
            }
        }
    }

}