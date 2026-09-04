using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MergeSlimePanel : ButtonUI
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text titleText;

    string key;
    Action<SlimeMergeData,string> onClick;
    SlimeMergeData slimeMergeData;
        public GameObject resourcesObject;
    public Image[] slimeImages;
    public void Set(SlimeMergeData mergeData, Action<SlimeMergeData,string> onClicked)
    {
        slimeMergeData = mergeData;
        onClick = onClicked;

        SlimeData data = SlimeManager.Instance.GetSlimeData(slimeMergeData.key);
        resourcesObject.SetActive(false);
        if(slimeMergeData.growth1SlimeKeys.Length == 2)
        {
            resourcesObject.SetActive(true);
            for(int i = 0; i < slimeImages.Length; i++)
            {
                slimeImages[i].sprite = SlimeManager.Instance.GetSlimeData(slimeMergeData.growth1SlimeKeys[i]).thum; 
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
        onClick?.Invoke(slimeMergeData, key);
    }
}
