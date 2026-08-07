
using System.Collections.Generic;
using UnityEngine;

public class NewEquipmentAbilityContainer : MonoBehaviour {

    public NewEquipmentAbilityChangeInfoPanel[] newAbilityChangeInfoPanels;
    public void SetContainer(EquipmentData oldEquipmentData, EquipmentData newEquipmentData, List<StatType> allStatTypes)
    {
        for(int i =0;i<newAbilityChangeInfoPanels.Length;i++)
        {
            if(i<newEquipmentData.abilities.Length)
            {
                newAbilityChangeInfoPanels[i].SetAbility(newEquipmentData.abilities[i], oldEquipmentData != null ? oldEquipmentData.GetEquipmentAbility(newEquipmentData.abilities[i].statType) : null);
            }
            else
            {
                newAbilityChangeInfoPanels[i].gameObject.SetActive(false);
            }
        }
    }
    
}