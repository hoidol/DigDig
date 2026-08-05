using UnityEngine;

public class EquipmentStatePanel : MonoBehaviour 
{
    public EquippedSlotPanel[] equippedSlotPanels;
    public void UpdatePanel()
    {
        foreach(EquippedSlotPanel panel in equippedSlotPanels)
        {
            panel.UpdatePanel();
        }
    }
}