using UnityEngine;

public class ItemShopManager : MonoSingleton<ItemShopManager> {
    public bool needToRefresh;


    public float refreshTimer;
    public float refreshTime =60;

    void Start()
    {
        refreshTimer = refreshTime;    
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
        return 10 + purchaseCount*2;
    }

    void Update()
    {
        if(!GameManager.Instance.isPlaying)
            return;
        

        if(refreshTimer > refreshTime)
        {
            needToRefresh = true;
            if (ItemShopCanvas.Instance.gameObject.activeSelf)
            {
                ItemShopCanvas.Instance.ResetItemShopProduct();
            }
            return;
        }
            
        refreshTimer += Time.deltaTime;
        needToRefresh = false;
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