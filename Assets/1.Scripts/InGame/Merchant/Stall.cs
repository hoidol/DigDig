using UnityEngine;

public class Stall : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    ItemData itemData;
    public void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        spriteRenderer.sprite = itemData.thumbnail;
    }
    public void Close()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ItemCanvas.Instance.OpenCanvas(new TryPurchaseItemEvent(itemData), (r) =>
            {
                GetComponentInParent<Merchant>().Disappear();

            });
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ItemCanvas.Instance.curItemData == itemData)
            {
                ItemCanvas.Instance.CloseCanvas();
            }
        }
    }
}
