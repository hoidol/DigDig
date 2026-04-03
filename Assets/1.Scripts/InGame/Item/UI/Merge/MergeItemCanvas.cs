using UnityEngine;

using System;
public class MergeItemCanvas : CanvasUI<MergeItemCanvas>
{
    [SerializeField] MergePanel mergePanel;
    [SerializeField] MergeResultPanel resultPanel;

    public void OpenCanvas(TryMergeItemEvent e, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        //아이템 머지 관련
        mergePanel.SetMergeItemData(e.mergeItemData);
    }


    public void OpenCanvas(MergedItemEvent e, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        //아이템 머지 관련
        resultPanel.SetMergedItemEvent(e);
    }
}
