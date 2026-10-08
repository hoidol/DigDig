using UnityEngine;

public class ItemShop : EventObject
{
    void Start()
    {
        
        GameEventBus.Subscribe<PurchaseItemEvent>(OnPurchaseItemEvent);
    }
    int purchaseCount;
    public override void Appear(Vector2 spawnPos)
    {purchaseCount = 0;
        base.Appear(spawnPos);
    }
    public override void StartEvent()
    {
        interacting = true;
        Time.timeScale = 0;
        ItemShopCanvas.Instance.OpenCanvas(() =>
        {
            if(purchaseCount == 3)
            {
                Destroy();    
            }
            Time.timeScale = 1;
            
        });

        // ItemShopManager.Instance.OpenCanvas(this);
    }

    void OnPurchaseItemEvent(PurchaseItemEvent e)
    {
        purchaseCount++;
    }
    
    //  public void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Player"))
    //     {
    //         interacting = true;
    //         ItemShopManager.Instance.OpenCanvas(this);
    //     }
    // }
}