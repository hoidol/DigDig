using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

public class ItemShopCanvas : CanvasUI<ItemShopCanvas>
{
    [SerializeField] ItemShopProductPanel[] itemShopProductPanels;
    [SerializeField] OwnItemPanel[] ownItemPanels;

    public override void Init()
    {
        if (init)
            return;
        init = true;
        itemShopProductPanels = GetComponentsInChildren<ItemShopProductPanel>();
        ownItemPanels = GetComponentsInChildren<OwnItemPanel>();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Init();
        ResetItemShopProduct();
        UpdateCanvas();
    }
    void OnEnable()
    {
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }
    void Start()
    {
        GameEventBus.Subscribe<PurchaseItemEvent>(OnPurchaseItemEvent);
    }
    void OnPurchaseItemEvent(PurchaseItemEvent e)
    {
        for (int i = 0; i < itemShopProductPanels.Length; i++)
        {
            if (!itemShopProductPanels[i].purchased)
            {
                return;
            }
        }
        // ResetItemShopProduct();
    }
    public void UpdateCanvas()
    {
        for (int i = 0; i < itemShopProductPanels.Length; i++)
        {
            itemShopProductPanels[i].UpdatePanel();
        }
        string[] itemKeys = Character.Instance.itemInventory.ownItemKeys;
        for (int i = 0; i < ownItemPanels.Length; i++)
        {
            ownItemPanels[i].SetItem(Character.Instance.itemInventory.GetItem(itemKeys[i]), i);
        }
    }

    //60초에 한번씩 갱신됨 - 광석 5개 소모해서 갱신 가능
    public void ResetItemShopProduct()
    {
        List<ItemData> itemDatas = ItemManager.Instance.GetDrawItems(3);
        for (int i = 0; i < itemDatas.Count; i++)
        {
            Debug.Log($"ItemShopCanvas ResetItemShopProduct() itemDatas[i] {itemDatas[i].key}");
            itemShopProductPanels[i].SetItemData(itemDatas[i]);
        }

        GameEventBus.Publish(new RefreshItemShopEvent());
    }

}