using UnityEngine;
using UnityEngine.UI;

public class EquipmentThumPanel : MonoBehaviour
{
    public Image thumImage;
    public Image bgImage;
    public Image gradeImage;
    public bool isNullToInactive;
    public void SetEquipmentData(EquipmentData equipmentData)
    {
        thumImage.enabled = equipmentData != null;
        gradeImage.enabled = equipmentData != null;
        if (equipmentData != null)
        {
            gameObject.SetActive(true);
            thumImage.sprite = equipmentData.thum;
            bgImage.color = Grade.GetGradeColor(equipmentData.grade);
            gradeImage.sprite = Grade.GetGradeSprite(equipmentData.grade);
        }
        else
        {
            if (isNullToInactive)
            {
                gameObject.SetActive(false);
            }
            bgImage.color = Color.gray;
        }
    }
}