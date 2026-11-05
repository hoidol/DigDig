using TMPro;
using UnityEngine;

public class DrawInfoPanel : MonoBehaviour
{
    public TMP_Text gradeText;
    public TMP_Text chacneText;
    public void SetInfo(GradeType grade, string chance)
    {
        gradeText.text = Grade.GetGradeText(grade);
        gradeText.color = Grade.GetGradeColor(grade);
        chacneText.text = $"{chance}%";
    }
}