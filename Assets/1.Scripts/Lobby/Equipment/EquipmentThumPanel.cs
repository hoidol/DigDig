using UnityEngine;
using UnityEngine.UI;

public class EquipmentThumPanel : MonoBehaviour 
{
    public Image thumImage;
    public Image bgImage;
    public Image gradeImage;
    public void SetEquipmentData(EquipmentData equipmentData)
    {
        thumImage.enabled = equipmentData != null;
        gradeImage.enabled = equipmentData != null;
        if(equipmentData != null)
        {
            thumImage.sprite = equipmentData.thum;    
            bgImage.color = EquipmentData.GetGradeColor(equipmentData.grade );
            gradeImage.sprite = EquipmentData.GetGradeSprite(equipmentData.grade);
        }
        else
        {
            bgImage.color =  Color.gray;
        }
    }    
}