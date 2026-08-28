using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectMergeMiniMePanel : ButtonUI
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text titleText;

    string key;
    Action<string> onClick;
    MiniMeMergeData miniMeMergeData;
    public GameObject resourcesObject;
    public Image[] miniMeImages;
    public void Set(MiniMeMergeData mergeData, Action<string> onClicked)
    {
        miniMeMergeData = mergeData;
        onClick = onClicked;

        MiniMeData data = MiniMeManager.Instance.GetMiniMeData(miniMeMergeData.key);
        resourcesObject.SetActive(false);
        if(miniMeMergeData.growth1MiniMeKeys.Length == 2)
        {
            resourcesObject.SetActive(true);
            for(int i = 0; i < miniMeImages.Length; i++)
            {
                miniMeImages[i].sprite = MiniMeManager.Instance.GetMiniMeData(miniMeMergeData.growth1MiniMeKeys[i]).thum; 
            }
        }
        icon.sprite = data.thum;
        titleText.text = data.Title;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public override void OnClickedBtn()
    {
        onClick?.Invoke(key);
    }
}
