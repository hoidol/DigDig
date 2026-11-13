using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MergeMinionPanel : ButtonUI
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text titleText;

    string key;
    Action<MinionMergeData,string> onClick;
    MinionMergeData minionMergeData;
        public GameObject resourcesObject;
    public Image[] minionImages;
    public void Set(MinionMergeData mergeData, Action<MinionMergeData,string> onClicked)
    {
        minionMergeData = mergeData;
        onClick = onClicked;

        MinionData data = MinionManager.Instance.GetMinionData(minionMergeData.key);
        resourcesObject.SetActive(false);
        if(minionMergeData.growth1MinionKeys.Length == 2)
        {
            resourcesObject.SetActive(true);
            for(int i = 0; i < minionImages.Length; i++)
            {
                minionImages[i].sprite = MinionManager.Instance.GetMinionData(minionMergeData.growth1MinionKeys[i]).thum; 
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
        onClick?.Invoke(minionMergeData, key);
    }
}
