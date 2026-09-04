using UnityEngine;

public class ItemShopManager : MonoSingleton<ItemShopManager>
{
    void Start()
    {
        purchaseCount = 0;
        GameEventBus.Subscribe<PurchaseItemEvent>(OnPurchaseItemEvent);
        GameEventBus.Subscribe<RefreshItemShopEvent>(OnRefreshItemShopEvent);
    }
    public int purchaseCount;
    public int itemShopOpenCount;
    public void OpenCanvas(ItemShop itemShop)
    {

        Time.timeScale = 0;
        ItemShopCanvas.Instance.OpenCanvas(() =>
        {
            itemShopOpenCount++;
            Time.timeScale = 1;
            itemShop.Destroy();
        });
        
    }
    void OnPurchaseItemEvent(PurchaseItemEvent e)
    {
        purchaseCount++;
    }
    void OnRefreshItemShopEvent(RefreshItemShopEvent e)
    {
    }

    public int GetPrice()
    {
        return GameSetting.INIT_ITEM_PRICE + purchaseCount * GameSetting.INCREASE_ITEM_PRICE;
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