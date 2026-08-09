using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class EquipmentSlotContainer : MonoBehaviour
{
    public EquipmentSlotPanel[] equipmentSlotPanels;
    public EquipmentSlotCategory slotCategory = EquipmentSlotCategory.All;
    public EquipmentSlotCategoryButton[] slotCategoryButtons;

    public void OpenContainer()
    {
        slotCategory = EquipmentSlotCategory.All;
    }

    public void ChangeSlotCategory(EquipmentSlotCategory slotCategory)
    {
        this.slotCategory = slotCategory;
        UpdateContainer();
    }

    public void UpdateContainer()
    {
        List<UserEquipment> userEquipments = UserManager.Instance.userEquipmentManager.userEquipmentData.userEquipments;
        List<UserEquipment> sortedUserEquipments = null; ;

        if (slotCategory == EquipmentSlotCategory.All)
        {
            sortedUserEquipments = userEquipments.OrderByDescending(e => e.equipmentData.grade).ThenBy(e => e.equipmentData.equipmentType).ThenBy(e => e.equipmentData.key).ToList();
        }
        else
        {
            sortedUserEquipments = userEquipments.Where(e => e.equipmentData.equipmentType == Enum.Parse<EquipmentType>(slotCategory.ToString())).OrderByDescending(e => e.equipmentData.grade).ThenBy(e => e.equipmentData.equipmentType).ThenBy(e => e.equipmentData.key).ToList();
        }

        for (int i = 0; i < equipmentSlotPanels.Length; i++)
        {
            if (i < sortedUserEquipments.Count)
            {
                equipmentSlotPanels[i].SetUserEquipment(sortedUserEquipments[i]);
            }
            else
            {
                equipmentSlotPanels[i].SetUserEquipment(null);
            }
        }

        for (int i = 0; i < slotCategoryButtons.Length; i++)
        {
            slotCategoryButtons[i].UpdateButon();
        }
    }

}

public enum EquipmentSlotCategory
{
    All,
    R_Hand, L_Hand, Head, Accessory
}