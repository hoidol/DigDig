using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ChangeItemCanvas : CanvasUI<ChangeItemCanvas>
{
    public ItemPanel targetItemPanel;
    public ChangeItemPanel[] ownItemPanels;

    public string targetItemKey;
    Action<bool> resultCallback;
    public void OpenCanvas(string itemKey, Action<bool> rCallback)
    {
        base.OpenCanvas(null);
        resultCallback = rCallback;
        this.targetItemKey = itemKey;
        if(ownItemPanels.Length<=0)
            ownItemPanels = GetComponentsInChildren<ChangeItemPanel>();

        string[] ownItemKeys = Character.Instance.itemInventory.ownItemKeys;
        
        for(int i = 0; i < ownItemPanels.Length; i++)
        {
            ownItemPanels[i].SetItem(Character.Instance.itemInventory.GetItem(ownItemKeys[i]),i);
            
        }

        targetItemPanel.SetItemData(ItemData.GetItemData(itemKey));
        OpenCanvas();
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();
        InvokeResult(false);
    }

    void InvokeResult(bool result)
    {
        var callback = resultCallback;
        resultCallback = null;
        callback?.Invoke(result);
    }
    
    public async UniTask Selected(int idx)
    {
        string removeKey = Character.Instance.itemInventory.ownItemKeys[idx];
        if (string.IsNullOrEmpty(removeKey))
            return;

        Character.Instance.RemoveItem(removeKey, idx);
        await Character.Instance.AddItem(targetItemKey);
        
        gameObject.SetActive(false);
        InvokeResult(true);
    }
}
