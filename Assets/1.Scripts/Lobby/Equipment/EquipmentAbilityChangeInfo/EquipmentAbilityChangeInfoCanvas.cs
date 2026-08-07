using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentAbilityChangeInfoCanvas : CanvasUI<EquipmentAbilityChangeInfoCanvas>
{
    public Image thumImage;
    public Image bgImage;
    UserEquipment oldEquipment;
    UserEquipment newEquipment;
    public OldEquipmentAbilityContainer oldAbilityContainer;
    public NewEquipmentAbilityContainer newAbilityContainer;
    public void OpenCanvas(UserEquipment oldEquipment, UserEquipment newEquipment, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        this.oldEquipment = oldEquipment;
        this.newEquipment = newEquipment;
        UpdateCanvas();
    }
    public void UpdateCanvas()
    { 
        EquipmentData oldEquipmentData =null;
        List<StatType> allStatTypes = new List<StatType>();
        if(oldEquipment != null)
        {
            oldEquipmentData = oldEquipment.equipmentData;
            thumImage.sprite = oldEquipmentData.thum;
            bgImage.color = EquipmentData.GetGradeColor(oldEquipmentData.grade);

            for(int i =0;i<oldEquipmentData.abilities.Length;i++)
            {
                allStatTypes.Add(oldEquipmentData.abilities[i].statType);
            }
        }
        EquipmentData newEquipmentData = newEquipment.equipmentData;

        for(int i =0;i<newEquipmentData.abilities.Length;i++)
        {
            if(allStatTypes.Contains(newEquipmentData.abilities[i].statType) == false)
                allStatTypes.Add(newEquipmentData.abilities[i].statType);
        }

        thumImage.sprite = newEquipmentData.thum;
        bgImage.color = EquipmentData.GetGradeColor(newEquipmentData.grade);

        oldAbilityContainer.SetContainer(oldEquipmentData, newEquipmentData, allStatTypes);
        newAbilityContainer.SetContainer(oldEquipmentData, newEquipmentData, allStatTypes);
    }

    public void OnClickedEquipButton()
    {
        UserManager.Instance.userEquipmentManager.EquiptUserEquipment(newEquipment);
        EquipmentCanvas.Instance.UpdateCanvas();
        UpdateCanvas();
    }
}