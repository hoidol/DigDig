
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OpenMergeItemButton : ButtonUI
{
    public TMP_Text titleText;
    public bool canOpen;
    public void UpdateButton()
    {
        List<MergeItemData> mergeItemDatas = ItemManager.Instance.GetMergeItemDataList();
        canOpen = mergeItemDatas.Count>0;
        titleText.text = $"합성({mergeItemDatas.Count})";
    }

    public override void OnClickedBtn()
    {
        if(!canOpen)
        {
            return;
        }
        MergeItemCanvas.Instance.OpenCanvas();
    }
}