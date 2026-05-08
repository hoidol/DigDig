using System;
using System.Collections.Generic;
using UnityEngine;

//아이템 하나 반드시 선택해야됌
public class SelectItemCanvas : CanvasUI<SelectItemCanvas>
{
    public SelectItemPanel[] selectItemPanels;
    //public int maxPickCount;
    [SerializeField] Grade grade;
    public void OpenCanvas(Grade grade, Action closeCallback)
    {
        base.OpenCanvas(closeCallback);

        Time.timeScale = 0;
        this.grade = grade;
        if (selectItemPanels == null || selectItemPanels.Length <= 0)
        {
            selectItemPanels = GetComponentsInChildren<SelectItemPanel>();

        }
        OnClickedReset();
        //List<ItemData> items = ItemManager.Instance.GetItems(grade, 3);

        // if (itemKeys != null && itemKeys.Length > 0)
        // {
        //     var pool = new List<ItemData>();
        //     foreach (var k in itemKeys)
        //     {
        //         var data = ItemManager.Instance.GetItemData(k);
        //         if (data != null) pool.Add(data);
        //     }
        //     // 랜덤 셔플 (겹치지 않게)
        //     items = new List<ItemData>();
        //     while (items.Count < 3 && pool.Count > 0)
        //     {
        //         int idx = UnityEngine.Random.Range(0, pool.Count);
        //         items.Add(pool[idx]);
        //         pool.RemoveAt(idx);
        //     }
        // }
        // else
        // {

        // }

        // for (int i = 0; i < selectItemPanels.Length; i++)
        // {
        //     bool hasItem = i < items.Count;
        //     selectItemPanels[i].gameObject.SetActive(hasItem);
        //     if (hasItem)
        //         selectItemPanels[i].SetItemData(items[i]);
        // }
    }
    public void OnClickedReset()
    {
        List<ItemData> items = ItemManager.Instance.GetItems(grade, 3);
        for (int i = 0; i < selectItemPanels.Length; i++)
        {
            bool hasItem = i < items.Count;
            selectItemPanels[i].gameObject.SetActive(hasItem);
            if (hasItem)
                selectItemPanels[i].SetItemData(items[i]);
        }
    }
    public void Selected(ItemData itemData)
    {
        Player.Instance.itemInventory.AddItem(itemData);
        CloseCanvas();
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();

        Time.timeScale = 1;
    }
}
