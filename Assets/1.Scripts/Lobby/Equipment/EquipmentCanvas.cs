using System;
using UnityEngine;

public class EquipmentCanvas : CanvasUI<EquipmentCanvas>  
{
   public EquipmentInventoryPanel inventoryPanel;
   public EquipmentStatePanel statePanel;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        inventoryPanel.UpdatePanel();
        statePanel.UpdatePanel();
    }
}