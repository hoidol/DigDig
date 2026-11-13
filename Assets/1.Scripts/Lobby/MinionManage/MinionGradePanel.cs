using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class MinionGradePanel : MonoBehaviour
{
    public TMP_Text titleText;
    public GameObject[] gradeBgObjs;
    public void SetMinionData(MinionData minionData)
    {
        titleText.text = Grade.GetGradeText(minionData.grade);
        int gIdx = (int)minionData.grade;
        for(int i = 0; i < gradeBgObjs.Length; i++)
        {
            if(i == gIdx)
            {
                gradeBgObjs[i].SetActive(true);
            }
            else
            {
                gradeBgObjs[i].SetActive(false);
            }            
        }
    }   


}
