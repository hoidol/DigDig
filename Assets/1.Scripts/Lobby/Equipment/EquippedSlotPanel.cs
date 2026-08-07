using UnityEngine;
using UnityEngine.UI;

public class EquippedSlotPanel : MonoBehaviour 
{
    public EquipmentType equipmentType;
    public EquipmentThumPanel equipmentThumPanel;
    public UserEquipment userEquipment;
    
    public void UpdatePanel()
    {
        userEquipment = UserManager.Instance.userEquipmentManager.GetEquippedUserEquipment(equipmentType);
        
        if (userEquipment != null)
        {
            equipmentThumPanel.SetEquipmentData(userEquipment.equipmentData);
        }
        else
        {
            equipmentThumPanel.SetEquipmentData(null);
        }
        
    }
    public void OnClickedPanel()
    {
        EquipmentInfoCanvas.Instance.OpenCanvas(userEquipment);
    }
    
}
