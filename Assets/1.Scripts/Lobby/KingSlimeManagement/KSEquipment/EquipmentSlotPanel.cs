using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotPanel : MonoBehaviour
{
    public EquipmentThumPanel equipmentThumPanel;
    public UserEquipment userEquipment;
    public GameObject equiptingPanel;

    public void SetUserEquipment(UserEquipment userEquipment)
    {
        this.userEquipment = userEquipment;
        if (userEquipment != null)
        {
            equiptingPanel.SetActive(userEquipment.equipped);
            equipmentThumPanel.SetEquipmentData(userEquipment.equipmentData);
        }
        else
        {
            equipmentThumPanel.SetEquipmentData(null);
        }
    }

    public void OnClickedSlot()
    {
        if (userEquipment != null)
        {
            if (userEquipment.equipped)
                EquipmentInfoCanvas.Instance.OpenCanvas(userEquipment);
            else
                EquipmentCompareCanvas.Instance.OpenCanvas(userEquipment);
        }
    }

}