using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>
{
    public Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();

    void Awake()
    {
        ItemData[] itemDatas = Resources.LoadAll<ItemData>("ItemData");
        foreach (ItemData itemData in itemDatas)
        {
            itemDataDic[itemData.key] = itemData;
        }
    }


    public List<ItemData> GetItems(Grade grade, int count)
    {
        var pool = itemDataDic.Values.Where(
            d =>
            {
                Item item = Player.Instance.inventory.GetItem(d.key);
                if (item != null)
                {
                    if (item.count >= ItemData.MAX_OWN_COUNT)
                        return false;
                }
                return d.grade <= grade;
            }

            ).ToList();
        var result = new List<ItemData>();

        while (result.Count < count && pool.Count > 0)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return result;
    }

    public ItemData GetItemData(string key)
    {
        return itemDataDic[key];
    }
}
public class TryPurchaseItemEvent
{
    public ItemData itemData;
    public Grade grade;
    public TryPurchaseItemEvent(ItemData iData, Grade grade)
    {
        itemData = iData;
        this.grade = grade;
    }
}
public class NearDropItemEvent
{
    public ItemData itemData;
    public NearDropItemEvent(ItemData iData)
    {
        itemData = iData;
    }
}

public class TryMergeItemEvent
{
    public Item item1;
    public Item item2;
    public TryMergeItemEvent(Item i1, Item i2)
    {
        item1 = i1;
        item2 = i2;
    }
}


public class MergedItemEvent
{
    public Item resourceItem1;
    public Item resourceItem2;
    public ItemData resultItemData;
    public MergedItemEvent(Item i1, Item i2, ItemData resultItemData)
    {
        resourceItem1 = i1;
        resourceItem2 = i2;
        this.resultItemData = resultItemData;
    }
}
