using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour 
{
    public Image thumImage;
    public Image bgImage;
    public UserEquipment userEquipment;
    public void SetUserEquipment(UserEquipment userEquipment)
    {
        this.userEquipment = userEquipment;
        thumImage.enabled = userEquipment != null;
        bgImage.color =Color.gray;
        if(userEquipment != null)
        {
            thumImage.sprite = userEquipment.equipmentData.thum;    
            bgImage.color = EquipmentData.GetGradeColor(userEquipment.equipmentData.grade );
        }
        
    }
    public void OnClickedSlot()
    {
        if(userEquipment != null)
        {
            EquipmentInfoCanvas.Instance.OpenCanvas(userEquipment);
        }
    }
    
}