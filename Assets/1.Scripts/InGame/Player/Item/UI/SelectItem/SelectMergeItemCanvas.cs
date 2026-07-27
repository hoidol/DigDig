using System;
using System.Collections.Generic;
using UnityEngine;

//아이템 하나 반드시 선택해야됌
public class SelectMergeItemCanvas : CanvasUI<SelectMergeItemCanvas>
{
    public List<MergeItemData> mergeItemDatas;
    public SelectMergeItemPanel[] selectMergeItemPanels;
    public void OpenCanvas(List<MergeItemData> mItemDatas, Action closeCallback)
    {
        base.OpenCanvas(closeCallback);

        mergeItemDatas = mItemDatas;

        Time.timeScale = 0;
        if (selectMergeItemPanels == null || selectMergeItemPanels.Length <= 0)
        {
            selectMergeItemPanels = GetComponentsInChildren<SelectMergeItemPanel>();

        }
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        for (int i = 0; i < selectMergeItemPanels.Length; i++)
        {
            selectMergeItemPanels[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < mergeItemDatas.Count; i++)
        {
            selectMergeItemPanels[i].gameObject.SetActive(true);
            selectMergeItemPanels[i].SetMergeItemData(mergeItemDatas[i]);
        }
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();
        Time.timeScale = 1;
    }
}
