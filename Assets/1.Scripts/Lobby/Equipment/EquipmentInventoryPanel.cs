using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class EquipmentInventoryPanel : MonoBehaviour 
{
    public EquipmentSlot[] equipmentSlots;
    public void UpdatePanel()
    {
        List<UserEquipment> userEquipments = UserManager.Instance.userEquipmentManager.userEquipmentData.userEquipments;
        List<UserEquipment> sortedUserEquipments = userEquipments.OrderByDescending(e => e.equipmentData.grade).ThenBy(e => e.equipmentData.equipmentType).ThenBy(e => e.equipmentData.key).ToList();
        for(int i = 0; i < equipmentSlots.Length; i++)
        {
            if(i < sortedUserEquipments.Count)
            {
                equipmentSlots[i].SetUserEquipment(sortedUserEquipments[i]);
            }else
            {
                equipmentSlots[i].SetUserEquipment(null);
            }
        }
    }
    
}