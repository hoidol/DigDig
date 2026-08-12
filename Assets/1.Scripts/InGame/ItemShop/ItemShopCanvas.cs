using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ItemShopCanvas : CanvasUI<ItemShopCanvas>
{
    [SerializeField] ItemShopProductPanel[] itemShopProductPanels;
    [SerializeField] OwnItemPanel[] ownItemPanels;
    [SerializeField] OpenMergeItemButton openMergeItemButton;
    [SerializeField] TMP_Text refreshTimeText;

    CancellationTokenSource cts;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        
        if (ItemShopManager.Instance.needToRefresh)
        {
            ResetItemShopProduct();
        }
    }
    void OnEnable()
    {
        Time.timeScale = 0;
        cts = new CancellationTokenSource();
        UpdateRefreshTimeTextLoop(cts.Token).Forget();
    }

    void OnDisable()
    {
        Time.timeScale = 1;
        cts?.Cancel();
        cts?.Dispose();
    }

    async UniTask UpdateRefreshTimeTextLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            UpdateRefreshTime();

            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
        }
    }
    void UpdateRefreshTime()
    {
        int remainSeconds = Mathf.CeilToInt(Mathf.Max(0, ItemShopManager.Instance.refreshTime - ItemShopManager.Instance.refreshTimer));
        refreshTimeText.text = $"갱신까지:{remainSeconds / 60:00}:{remainSeconds % 60:00}";
    }

    public void UpdateCanvas()
    {
        for(int i = 0; i < itemShopProductPanels.Length; i++)
        {
            itemShopProductPanels[i].UpdatePanel();
        }   
        List<Item> items = Character.Instance.itemInventory.curItems;
        for(int i = 0; i < ownItemPanels.Length; i++)
        {
            if (i < items.Count)
            {
                ownItemPanels[i].SetItem(items[i],i);
            }
            else
            {
                ownItemPanels[i].SetItem(null,i);
            }
        }
        openMergeItemButton.UpdateButton();
    }

    //60초에 한번씩 갱신됨 - 광석 5개 소모해서 갱신 가능
    public void ResetItemShopProduct()
    {
        List<ItemData> itemDatas = ItemManager.Instance.GetDrawItems(3);
        for(int i = 0; i < itemDatas.Count; i++)
        {
            itemShopProductPanels[i].SetItemData(itemDatas[i]);
        }
        
        UpdateRefreshTime();
        GameEventBus.Publish(new RefreshItemShopEvent());
    }

    public void OnClickedResetItem()
    {
        Character.Instance.AddOrePiece(-5);
        ResetItemShopProduct();
    }
}