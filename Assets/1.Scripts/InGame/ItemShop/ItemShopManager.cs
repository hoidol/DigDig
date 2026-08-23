using UnityEngine;

public class ItemShopManager : MonoSingleton<ItemShopManager>
{
    public bool needToRefresh;


    public float refreshTimer;
    public float refreshTime = 90;

    void Start()
    {
        refreshTimer = 0;
        needToRefresh = true;
        purchaseCount = 0;
        GameEventBus.Subscribe<PurchaseItemEvent>(OnPurchaseItemEvent);
        GameEventBus.Subscribe<RefreshItemShopEvent>(OnRefreshItemShopEvent);
    }
    public int purchaseCount;

    void OnPurchaseItemEvent(PurchaseItemEvent e)
    {
        purchaseCount++;
    }
    void OnRefreshItemShopEvent(RefreshItemShopEvent e)
    {
        refreshTimer = 0;
        needToRefresh = false;
    }
    public int GetPrice()
    {
        return GameSetting.INIT_ITEM_PRICE + purchaseCount * GameSetting.INCREASE_ITEM_PRICE;
    }

    void Update()
    {
        if (!GameManager.Instance.isPlaying)
            return;


        if (refreshTimer > refreshTime && !needToRefresh)
        {
            needToRefresh = true;
            if (ItemShopCanvas.Instance.gameObject.activeSelf)
            {
                ItemShopCanvas.Instance.ResetItemShopProduct();
            }
            return;
        }


        refreshTimer += Time.unscaledDeltaTime;
    }
}
public class PurchaseItemEvent
{
    public ItemData purchaseItemData;
    public PurchaseItemEvent(ItemData itemData)
    {
        purchaseItemData = itemData;
    }
}
public class RefreshItemShopEvent
{

    public RefreshItemShopEvent()
    {
    }
}