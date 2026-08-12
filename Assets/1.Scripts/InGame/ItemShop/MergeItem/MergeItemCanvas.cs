using System;
using System.Collections.Generic;
using UnityEngine;

public class MergeItemCanvas : CanvasUI<MergeItemCanvas>
{
    [SerializeField] MergeItemPanel[] mergeItemPanels;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        UpdateCanvas();
    }
    
    public void UpdateCanvas()
    {
        List<MergeItemData> mergeItemDatas = ItemManager.Instance.GetMergeItemDataList();
        for(int i = 0; i < mergeItemPanels.Length; i++)
        {
            if (i < mergeItemDatas.Count)
            {
                mergeItemPanels[i].SetMergeItemData(mergeItemDatas[i]);
            }
            else
            {
                mergeItemPanels[i].SetMergeItemData(null);
            }
            
        }   
    }
}