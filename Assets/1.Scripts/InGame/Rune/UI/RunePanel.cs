using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RunePanel : RunePanelOnlyImage
{

    [Header("비어있으면 설정안됨")]
    public TMP_Text titleText;
    [Header("비어있으면 설정안됨")]
    public TMP_Text descText;
    // public GameObject mergePanel;
    // public RunePanelOnlyImage[] mergeItemPanels;

    public override void SetRuneData(RuneData runeData)
    {
        base.SetRuneData(runeData);
        if(runeData != null)
        {
            if (titleText != null)
                titleText.text = runeData.Title;

            if (descText != null)
                descText.text = runeData.GetDescription();
        }
        else
        {
            if (titleText != null)
            titleText.text = null;
            if (descText != null)
            descText.text = null;
        }
        
    }
}
