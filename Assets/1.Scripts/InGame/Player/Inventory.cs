using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Inventory : MonoBehaviour
{
    public List<Item> equippedItems = new List<Item>();

    void Awake()
    {
        GameEventBus.Subscribe<AddedItemEvent>(AddedItemEvent);
    }
    void AddedItemEvent(AddedItemEvent e)
    {
        AddItem(e.itemData);
    }

    public void AddItem(ItemData itemData, Grade grade = Grade.Normal)
    {
        Item item = equippedItems.Where(e => e.key == itemData.key).FirstOrDefault();
        if (item != null)
        {
            item.count++;
            item.UpdateItem(); //아이템 장착 중일 시 업데이트
            return;
        }

        item = Instantiate(itemData.itemPrefab, transform);
        item.key = itemData.key;
        item.count = (int)grade + 1;
        item.OnEquip(Player.Instance); //아이템 장착 
        equippedItems.Add(item);
        SortingItem();
        CheckMerge();
    }
    public void SortingItem() //적용 순서를 고려
    {
        equippedItems = equippedItems.OrderBy(e => e.GetItemData().applyOrder).ToList();
    }

    public void ReleaseItem(string key)
    {
        Item item = equippedItems.Where(e => e.key == key).FirstOrDefault();
        item.count--;
        if (item.count == 0)
        {
            equippedItems.Remove(item);
            Destroy(item.gameObject);
        }
        SortingItem();
        CheckMerge();
    }


    public Item GetItem(string key)
    {
        return equippedItems.Where((e) => e.key == key).FirstOrDefault();
    }

    public void CheckMerge()
    {

    }

    // public string TakeOut()
    // {
    //     if (playerInventories.Count <= 0)
    //         return null;
    //     string key = playerInventories[0].key;
    //     playerInventories[0].count--;
    //     if (playerInventories[0].count == 0)
    //     {
    //         playerInventories.RemoveAt(0);
    //     }

    //     return key;
    // }
}

[System.Serializable]
public class PlayerItem
{
    public string key;
    public int count;
}