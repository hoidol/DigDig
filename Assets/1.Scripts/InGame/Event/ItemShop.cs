using UnityEngine;

public class ItemShop : EventObject
{
     public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interacting = true;
            ItemShopManager.Instance.OpenCanvas(this);
        }
    }
}