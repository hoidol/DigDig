using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroppedItem : MonoBehaviour
{
    public static List<DroppedItem> list = new List<DroppedItem>();
    static DroppedItem droppedItemPrefab;
    public static DroppedItem Instantiate()
    {
        DroppedItem droppedItem = GetDroppedItemInPooling();
        return droppedItem;
    }
    static DroppedItem GetDroppedItemInPooling()
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].gameObject.activeSelf)
                continue;
            list[i].gameObject.SetActive(true);
            return list[i];
        }
        if (droppedItemPrefab == null)
        {
            droppedItemPrefab = Resources.Load<DroppedItem>("UI/DroppedItem");
        }

        DroppedItem dText = Instantiate(droppedItemPrefab);
        list.Add(dText);
        return dText;
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
