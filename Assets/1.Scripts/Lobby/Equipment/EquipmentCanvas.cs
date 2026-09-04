using System;
using UnityEngine;

public class EquipmentCanvas : BaseLobbyCanvas
{
   public EquipmentStatePanel statePanel;
   public EquipmentSlotContainer slotContainer;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        statePanel.OpenPanel();
        slotContainer.OpenContainer();
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        statePanel.UpdatePanel();
        slotContainer.UpdateContainer();
    }
}