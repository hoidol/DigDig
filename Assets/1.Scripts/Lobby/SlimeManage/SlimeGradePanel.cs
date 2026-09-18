using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SlimeGradePanel : MonoBehaviour
{
    public TMP_Text titleText;
    public Image bgImage;
    public void SetSlimeData(SlimeData slimeData)
    {
        titleText.text = Grade.GetGradeText(slimeData.grade);
        bgImage.color = Grade.GetGradeColor(slimeData.grade);
    }   


}
