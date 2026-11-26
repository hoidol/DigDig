
using UnityEngine;
using UnityEngine.UI;

public class RuneDisplayPanel : MonoBehaviour
{
    public Image thumImage;
    //public Image bgImage;

    public RuneData runeData
    {
        get;
        private set;
    }
    public virtual void SetRuneData(RuneData runeData)
    {
        // bgImage.color = RuneData.GetGradeColor(itemData.grade);
        thumImage.sprite = runeData.thum;
        // thumImage.color = itemData.color;
    }


}