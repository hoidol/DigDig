using UnityEngine;
using UnityEngine.UI;

public class EquippedSlotPanel : MonoBehaviour 
{
    public EquipmentType equipmentType;
    public Image thumImage;
    public Image bgImage;
    public UserEquipment userEquipment;
    public Image gradeImage;
    public void UpdatePanel()
    {
        userEquipment = UserManager.Instance.userEquipmentManager.GetEquippedUserEquipment(equipmentType);
        thumImage.enabled = userEquipment != null;
        if (userEquipment != null)
        {
            thumImage.sprite = userEquipment.equipmentData.thum;
            bgImage.color = EquipmentData.GetGradeColor(userEquipment.equipmentData.grade );
        }
        bgImage.color =  Color.gray;
        
    }
    
}
