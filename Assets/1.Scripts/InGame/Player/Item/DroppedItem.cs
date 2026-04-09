using UnityEngine;
using System.Collections.Generic;

public class DroppedItem : MonoBehaviour
{
    static readonly Stack<DroppedItem> pool = new();
    static DroppedItem prefab;

    public static DroppedItem Instantiate()
    {
        if (pool.Count > 0)
        {
            DroppedItem item = pool.Pop();
            item.gameObject.SetActive(true);
            return item;
        }
        if (prefab == null)
            prefab = Resources.Load<DroppedItem>("UI/DroppedItem");
        return Instantiate(prefab);
    }

    public void Release()
    {
        gameObject.SetActive(false);
        pool.Push(this);
    }

    public ItemData itemData;
    public void Drop(ItemData iData)
    {
        itemData = iData;
        GetComponentInChildren<SpriteRenderer>().sprite = itemData.thumbnail;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ItemCanvas.Instance.OpenCanvas(new NearDropItemEvent(itemData), (r) =>
            {

            });
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ItemCanvas.Instance.curItemData == itemData)
            {
                ItemCanvas.Instance.CloseCanvas(itemData.key);
            }
        }
    }

}
