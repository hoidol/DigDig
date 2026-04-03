using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoSingleton<ItemManager>
{
    public Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    public MergeItemData[] mergeItemDatas;

    void Awake()
    {
        ItemData[] itemDatas = Resources.LoadAll<ItemData>("ItemData");
        foreach (ItemData itemData in itemDatas)
        {
            itemDataDic[itemData.key] = itemData;
        }

        mergeItemDatas = Resources.LoadAll<MergeItemData>("MergeItemData");
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

                    if (item.count >= d.maxOwnCount)
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

    public MergeItemData GetMergeItemData(params string[] itemKeys)
    {
        return mergeItemDatas.FirstOrDefault(d =>
            itemKeys.All(key => d.resourceItemKeys.Contains(key)) &&
            d.resourceItemKeys.Length == itemKeys.Length);
    }
}


public class TryPurchaseItemEvent
{
    public ItemData itemData;
    public TryPurchaseItemEvent(ItemData iData)
    {
        itemData = iData;
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
    public MergeItemData mergeItemData;

    public TryMergeItemEvent(MergeItemData data)
    {
        mergeItemData = data;
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
