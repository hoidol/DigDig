using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;


public class ItemShopCanvas : CanvasUI<ItemShopCanvas>
{
    [SerializeField] ItemProductInfoPanel  infoPanel;
    [SerializeField] ItemProductPanel[] itemProductPanels;
    public GameObject mainCloseButton;

    public override void Init()
    {
        if (init)
            return;
        init = true;
        itemProductPanels = GetComponentsInChildren<ItemProductPanel>();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateCanvas();
    }    void Start()
    {
        GameEventBus.Subscribe<PurchaseItemEvent>(OnPurchaseItemEvent);
    }
    
    void OnPurchaseItemEvent(PurchaseItemEvent e)
    {
        UpdateCanvas();
        for (int i = 0; i < itemProductPanels.Length; i++)
        {
            if (!itemProductPanels[i].purchased)
            {
                return;
            }
        }

        ResetItemShopProduct();
    }

    public void ShowItemInfo(ItemProductPanel iPPanel)
    {
        mainCloseButton.SetActive(false);
        infoPanel.ShowItemInfo(iPPanel, () =>
        {
            mainCloseButton.SetActive(true);
        });
    }

    public void UpdateCanvas()
    {
        for (int i = 0; i < itemProductPanels.Length; i++)
        {
            itemProductPanels[i].UpdatePanel();
        }
    }

    //60초에 한번씩 갱신됨 - 광석 5개 소모해서 갱신 가능
    public void ResetItemShopProduct()
    {
        List<ItemData> itemDatas = ItemManager.Instance.GetDrawItems(3);
        for (int i = 0; i < itemDatas.Count; i++)
        {
            Debug.Log($"ItemShopCanvas ResetItemShopProduct() itemDatas[i] {itemDatas[i].key}");
            itemProductPanels[i].SetItemData(itemDatas[i]);
        }

        GameEventBus.Publish(new RefreshItemShopEvent());
    }

}