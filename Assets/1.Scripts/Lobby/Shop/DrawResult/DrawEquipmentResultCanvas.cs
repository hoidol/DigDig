using System;
using UnityEngine;
namespace Lobby
{
    public class DrawEquipmentResultCanvas : DrawResultCanvas<DrawEquipmentResultCanvas> 
{
    public EquipmentThumPanel[] equipmentThumPanels;
    public override void OpenCanvas(string[] pickedKeys, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < equipmentThumPanels.Length; i++)
        {
            if(i < pickedKeys.Length)
            {
                equipmentThumPanels[i].gameObject.SetActive(true);
                equipmentThumPanels[i].SetEquipmentData(EquipmentManager.Instance.GetEquipmentData(pickedKeys[i]));
            }
            else
            {
                equipmentThumPanels[i].gameObject.SetActive(false);
            }
            
        }
        
    }
}
}
