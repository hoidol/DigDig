using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefenceTypePanel : MonoBehaviour 
{
    public TMP_Text titleText;
    public TMP_Text descText;
    public Image thumImage;
    public void SetDefenceType(DefenceType defenceType)
    {
        
        titleText.text = $"유형:{DefenceInfo.Title(defenceType)}";
        descText.text = DefenceInfo.Desc(defenceType);
        thumImage.sprite = DefenceInfo.GetSprite(defenceType);
    }
}