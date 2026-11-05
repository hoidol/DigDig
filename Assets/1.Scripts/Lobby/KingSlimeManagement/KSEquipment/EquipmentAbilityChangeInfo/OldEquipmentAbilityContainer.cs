
using System.Collections.Generic;
using UnityEngine;

public class OldEquipmentAbilityContainer : MonoBehaviour {

    public OldEquipmentAbilityChangeInfoPanel[] oldAbilityChangeInfoPanels;
    public void SetContainer(EquipmentData oldEquipmentData, EquipmentData newEquipmentData, List<StatType> allStatTypes)
    {
        if(oldEquipmentData== null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(false);
        for(int i =0;i<oldAbilityChangeInfoPanels.Length;i++)
        {
            if(i<oldEquipmentData.abilities.Length)
            {
                oldAbilityChangeInfoPanels[i].SetAbility(oldEquipmentData.abilities[i]);
            }
            else
            {
                oldAbilityChangeInfoPanels[i].gameObject.SetActive(false);
            }
        }
    }
    
}