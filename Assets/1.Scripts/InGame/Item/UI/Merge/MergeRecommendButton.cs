using UnityEngine;
using UnityEngine.UI;

public class MergeRecommendButton : ButtonUI
{
    public Image gradeBg;
    public Image thum;
    MergeItemData mergeItemData;
    public void SetMergeItemData(MergeItemData data)
    {

        mergeItemData = data;
        ItemData resultItemData = ItemManager.Instance.GetItemData(data.resultItemKey);
        thum.sprite = resultItemData.thumbnail;
        gradeBg.color = ItemData.GetGradeColor(resultItemData.grade);
    }

    public override void OnClickedBtn()
    {
        MergeItemCanvas.Instance.OpenCanvas(new TryMergeItemEvent(mergeItemData));
    }
}
