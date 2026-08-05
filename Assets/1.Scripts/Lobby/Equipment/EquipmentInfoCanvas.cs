using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentInfoCanvas : CanvasUI<EquipmentInfoCanvas>
{
    public Image thumImage;
    public Image bgImage;
    UserEquipment userEquipment;
    public void OpenCanvas(UserEquipment userEquipment, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        this.userEquipment = userEquipment;
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        if (userEquipment != null)
        {
            thumImage.sprite = userEquipment.equipmentData.thum;
            bgImage.color = EquipmentData.GetGradeColor(userEquipment.equipmentData.grade);
        }
    }

    public void OnClickedEquipButton()
    {
        UserManager.Instance.userEquipmentManager.EquiptUserEquipment(userEquipment);
        EquipmentCanvas.Instance.UpdateCanvas();
        UpdateCanvas();
    }
}