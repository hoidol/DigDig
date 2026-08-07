using System;
using UnityEngine;

public class DrawEquipmentResultCanvas : CanvasUI<DrawEquipmentResultCanvas> 
{
    public EquipmentThumPanel[] equipmentThumPanels;
    public void OpenCanvas(string[] pickedEquipmentKeys, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < equipmentThumPanels.Length; i++)
        {
            if(i < pickedEquipmentKeys.Length)
            {
                equipmentThumPanels[i].gameObject.SetActive(true);
                equipmentThumPanels[i].SetEquipmentData(EquipmentManager.Instance.GetEquipmentData(pickedEquipmentKeys[i]));
            }
            else
            {
                equipmentThumPanels[i].gameObject.SetActive(false);
            }
            
        }
        
    }
}