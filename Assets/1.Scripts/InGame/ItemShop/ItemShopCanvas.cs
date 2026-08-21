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
    public override void Init()
    {
        if(init)
            return ;
        init= true;
        itemShopProductPanels = GetComponentsInChildren<ItemShopProductPanel>();
        ownItemPanels = GetComponentsInChildren<OwnItemPanel>();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Init();
        
        if (ItemShopManager.Instance.needToRefresh)
        {
            ResetItemShopProduct();
        }
        UpdateCanvas();
    }
    void OnEnable()
    {
        Time.timeScale = 0;
        cts = new CancellationTokenSource();
        UpdateRefreshTime();
        UpdateRefreshTimeTextLoop(cts.Token).Forget();
    }

    void OnDisable()
    {
        Time.timeScale = 1;
        cts?.Cancel();
        cts?.Dispose();
    }
    void Start()
    {
        GameEventBus.Subscribe<PurchaseItemEvent>(OnPurchaseItemEvent);
    }
    void OnPurchaseItemEvent(PurchaseItemEvent e)
    {
        for(int i = 0; i < itemShopProductPanels.Length; i++)
        {
            if (!itemShopProductPanels[i].purchased)
            {
                return;
            }
        }
        // ResetItemShopProduct();
    }

    async UniTask UpdateRefreshTimeTextLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            UpdateRefreshTime();
            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.UnscaledDeltaTime, cancellationToken: token);
        }
    }
    void UpdateRefreshTime()
    {
        int remainSeconds = Mathf.CeilToInt(Mathf.Max(0, ItemShopManager.Instance.refreshTime - ItemShopManager.Instance.refreshTimer));

        Debug.Log($"UpdateRefreshTime 갱신해 {remainSeconds}");
        refreshTimeText.text = $"{TranslateManager.GetText("UtillRefresh")}:{remainSeconds / 60:00}:{remainSeconds % 60:00}";
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
        Character.Instance.AddCoin(-5);
        ResetItemShopProduct();
    }
}