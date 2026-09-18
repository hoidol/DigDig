
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlimeStatPanel : MonoBehaviour
{
    // public SlimeStatType statType;
    public TMP_Text titleText;
    public TMP_Text valueText;
    public Image thumImage;
    public void SetSlimeStat(SlimeStat slimeStat,int mergeLv)
    {
        thumImage.sprite = slimeStat.Thum;
        titleText.text = slimeStat.Title;
        valueText.text = slimeStat.GetValueToString(mergeLv);
    }
}
