using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class EquipmentSlotCategoryButton : ButtonUI
{
    public EquipmentSlotCategory slotCategory;

    public void UpdateButon()
    {
        if (GetComponentInParent<EquipmentSlotContainer>().slotCategory == slotCategory)
        {
            button.image.color = ColorSetting.activeColor;
        }
        else
        {
            button.image.color = Color.white;
        }
    }
    public override void OnClickedBtn()
    {
        GetComponentInParent<EquipmentSlotContainer>().ChangeSlotCategory(slotCategory);
    }
}
